import { Component, AfterViewInit, HostListener, HostBinding, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import mapboxgl from 'mapbox-gl';
import { MatIcon } from '@angular/material/icon';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { SidebarCommunicationService, RegionService, LandfillService } from '../../services';
import { RegionDto, LandfillDto, MonitoringDto } from '../../DTOs';
import { MonitoringService } from '../../services/monitoring-service';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, NgFor, NgIf, TranslatePipe, MatIcon],
  templateUrl: './map-component.html',
  styleUrls: ['./map-component.css'],
})
export class MapComponent implements AfterViewInit {
  // Mapbox
  map!: mapboxgl.Map;
  private readonly accessToken =
    'pk.eyJ1IjoiYW5kamVsYW1yZGphIiwiYSI6ImNtZ2k1eGl0dTA1YnUybHF4ZDdmZnlqNTQifQ.B-W-2BJqVGziYH-15nCvIA';

  // Services
  private sidebarService = inject(SidebarCommunicationService);
  private regionService = inject(RegionService);
  private landfillService = inject(LandfillService);
  private monitoringService = inject(MonitoringService);

  // UI state
  @HostBinding('class.sidebar-open') sidebarOpen = true;
  hoveredCard: number | null = null;

  // Data
  selectedRegionData: RegionDto | null = null;
  selectedLandfills: LandfillDto[] = [];
  selectedLandfill: LandfillDto | null = null;
  latestMonitoring: MonitoringDto | null = null;

  // Regions (bounds for zoom). Keys are FE-local; display names map below.
  regions: Record<string, mapboxgl.LngLatBoundsLike> = {
    WholeSerbia: [
      [18.7, 42.2],
      [22.6, 46.2],
    ],
    Vojvodina: [
      [18.6, 45.0],
      [21.1, 46.2],
    ],
    Belgrade: [
      [20.2, 44.5],
      [20.7, 44.95],
    ],
    WesternSerbia: [
      [18.6, 43.4],
      [20.2, 44.6],
    ],
    SumadijaAndPomoravlje: [
      [20.3, 43.6],
      [21.4, 44.3],
    ],
    EasternSerbia: [
      [21.4, 43.6],
      [22.6, 44.8],
    ],
    SouthernSerbia: [
      [20.6, 42.3],
      [22.4, 43.5],
    ],
    KosovoAndMetohija: [
      [20.3, 41.8],
      [22.0, 43.1],
    ],
  };

  // FE display (Serbian) used for UI only.
  regionDisplayNames: Record<string, string> = {
    Vojvodina: 'Vojvodina',
    Belgrade: 'Beograd',
    WesternSerbia: 'Zapadna Srbija',
    SumadijaAndPomoravlje: 'Šumadija i Pomoravlje',
    EasternSerbia: 'Istočna Srbija',
    SouthernSerbia: 'Južna Srbija',
    KosovoAndMetohija: 'Kosovo i Metohija',
  };

  regionKeys = Object.keys(this.regions);

  // Use this to build translation keys and to derive the API key from the display name.
  getRegionKey(name: string): string {
    // Normalize: lowercase, strip spaces, fold common diacritics
    return name
      .toLowerCase()
      .replace(/\s+/g, '')
      .replace(/š/g, 's')
      .replace(/đ/g, 'dj')
      .replace(/č|ć/g, 'c')
      .replace(/ž/g, 'z');
  }

  tooltipText: string | null = null;
  tooltipPosition = { x: 0, y: 0 };

  ngAfterViewInit(): void {
    mapboxgl.accessToken = this.accessToken;
    this.initializeMap();
  }

  private initializeMap(): void {
    this.map = new mapboxgl.Map({
      container: 'map',
      style: 'mapbox://styles/mapbox/satellite-streets-v12',
      center: [20.9, 43.9],
      zoom: 2,
    });

    this.map.on('load', () => {
      this.map.setFog({
        'horizon-blend': 0.2,
        color: '#242B4B',
        'high-color': '#161B36',
        'space-color': '#0B1026',
        'star-intensity': 0.6,
      });
    });
  }

  @HostListener('window:resize') onResize() {
    this.map?.resize();
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
    this.sidebarService.toggleSidebar();
  }

  /** Map FE regionKey -> display name -> API key (canonical). */
  private regionKeyForApi(feKey: string): string {
    const display = this.regionDisplayNames[feKey] ?? feKey; // e.g. "Beograd"
    return this.getRegionKey(display); // e.g. "beograd"
  }

  onRegionSelect(event: Event): void {
    const feKey = (event.target as HTMLSelectElement).value; // e.g. "Belgrade"
    if (!feKey) return;

    this.zoomToRegion(feKey);

    const apiKey = this.regionKeyForApi(feKey); // e.g. "beograd"

    const displayName = this.regionDisplayNames[feKey] ?? feKey; // e.g. "Beograd"

    this.selectedLandfill = null;
    this.latestMonitoring = null;
    this.selectedRegionData = null;

    this.regionService
      .getRegionByName(displayName)
      .subscribe((region) => (this.selectedRegionData = region));

    this.landfillService
      .getLandfillsByRegion(apiKey)
      .subscribe((lfs) => (this.selectedLandfills = lfs));
  }

  private zoomToRegion(feKey: string): void {
    const bounds = this.regions[feKey];
    if (bounds) {
      this.map.fitBounds(bounds, { padding: 50, duration: 1200, bearing: 0 });
    }
  }

  // Place markers for currently selected landfills (call this after selectedLandfills updates if you want markers)
  private initializeLandfills(): void {
    for (const landfill of this.selectedLandfills) {
      new mapboxgl.Marker({ color: '#FF5B5B', scale: 0.8 })
        // Mapbox requires [lng, lat] !
        .setLngLat([landfill.longitude, landfill.latitude])
        .setPopup(
          new mapboxgl.Popup({ closeButton: true, closeOnClick: false }).setHTML(
            `<strong style="display:block;margin-bottom:8px;font-size:16px;">${landfill.id}</strong>`
          )
        )
        .addTo(this.map);
    }
  }

  onLandfillClick(lf: LandfillDto): void {
    this.selectedLandfill = lf;
    this.monitoringService.getLatestMonitoring(lf.id).subscribe((m) => (this.latestMonitoring = m));

    // We don't have a bbox; center on point.
    this.landfillZoomIn(lf.latitude, lf.longitude, lf.latitude, lf.longitude);
  }

  onCardsMouseMove(event: MouseEvent): void {
    const container = (event.currentTarget as HTMLElement).closest(
      '.landfill-cards-scroll'
    ) as HTMLElement;
    if (event.buttons === 1) container.scrollLeft -= event.movementX;
  }

  showTooltip(event: MouseEvent, text: string): void {
    this.tooltipText = text;
    const rect = (event.target as HTMLElement).getBoundingClientRect();
    this.tooltipPosition = { x: rect.left + rect.width / 2, y: rect.top - 12 };
  }

  hideTooltip(): void {
    this.tooltipText = null;
  }

  private landfillZoomIn(nwLat: number, nwLng: number, seLat: number, seLng: number) {
    // If caller passes same point twice, just flyTo center.
    if (nwLat === seLat && nwLng === seLng) {
      this.map.flyTo({ center: [seLng, seLat], zoom: 12, speed: 0.8, curve: 1.2 });
      return;
    }

    // Bounds must be [[minLng,minLat],[maxLng,maxLat]]
    const minLng = Math.min(nwLng, seLng);
    const maxLng = Math.max(nwLng, seLng);
    const minLat = Math.min(nwLat, seLat);
    const maxLat = Math.max(nwLat, seLat);

    const bounds: mapboxgl.LngLatBoundsLike = [
      [minLng, minLat],
      [maxLng, maxLat],
    ];
    this.map.fitBounds(bounds, { padding: 50, duration: 1200, bearing: 0 });
  }
}
