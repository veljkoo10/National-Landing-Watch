import { Component, AfterViewInit, HostListener, HostBinding, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import mapboxgl from 'mapbox-gl';
import { MatIcon } from '@angular/material/icon';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { SidebarCommunicationService, RegionService, LandfillService } from '../../services';
import { RegionDto, MonitoringDto } from '../../DTOs';
import { MonitoringService } from '../../services/monitoring-service';
import { FormsModule } from '@angular/forms';
import { LandfillSite } from '../../DTOs/landfillSiteAll-DTO';
import { LandfillCategory } from '../../DTOs/Enums/LandfillCategory';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, NgFor, NgIf, TranslatePipe, MatIcon, FormsModule],
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
  selectedLandfills: LandfillSite[] = [];
  selectedLandfill: LandfillSite | null = null;
  // latestMonitoring: MonitoringDto | null = null;
  latestMonitorings: Record<number, MonitoringDto> = {};
  mapType: 'satellite' | 'heatmap' = 'satellite';
  markers: mapboxgl.Marker[] = [];
  allLandfills: LandfillSite[] = [];
  heatmapPopup: mapboxgl.Popup | null = null;
  isDropdownOpen = false;

  emissions = this.allLandfills.map((lf) => this.latestMonitorings[lf.id]?.ch4Tons ?? 0);
  minEmission = Math.min(...this.emissions);
  maxEmission = Math.max(...this.emissions);

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
    WholeSerbia: 'Srbija',
    Vojvodina: 'Vojvodina',
    Belgrade: 'Beograd',
    WesternSerbia: 'Zapadna Srbija',
    SumadijaAndPomoravlje: 'Šumadija i Pomoravlje',
    EasternSerbia: 'Istočna Srbija',
    SouthernSerbia: 'Južna Srbija',
    KosovoAndMetohija: 'Kosovo i Metohija',
  };

  typeColors: Record<LandfillCategory | number, string> = {
    [LandfillCategory.Illegal]: '#FF5B5B', // Red
    [LandfillCategory.NonSanitary]: '#FF9800', // Orange
    [LandfillCategory.Sanitary]: '#2196F3', // Blue
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
    this.landfillService.getAllLandfills().subscribe((landfills) => {
      this.allLandfills = landfills;
      // Fetch latest monitoring for all landfills
      // landfills.forEach((lf) => {
      //   // this.monitoringService.getLatestMonitoring(lf.id).subscribe((monitoring) => {
      //   //   this.latestMonitorings[lf.id] = monitoring;
      //   // });
      // });
      this.initializeMap();
    });
  }

  private initializeMap(): void {
    this.map = new mapboxgl.Map({
      container: 'map',
      style: 'mapbox://styles/mapbox/satellite-streets-v12',
      center: [20.9, 43.9],
      zoom: 2,
    });

    this.map.on('load', () => {
      this.landfillService.getAllLandfills().subscribe((landfills) => {
        // this.selectedLandfills = landfills;
        this.initializeLandfills();
      });
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
    // this.latestMonitoring = null;
    this.selectedRegionData = null;

    if (feKey === 'WholeSerbia') {
      this.landfillService.getAllLandfills().subscribe((lfs) => {
        this.selectedLandfills = lfs;
        this.initializeLandfills();
      });
    } else {
      const apiKey = this.regionKeyForApi(feKey);
      const displayName = this.regionDisplayNames[feKey] ?? feKey;

      this.regionService
        .getRegionByName(displayName)
        .subscribe((region) => (this.selectedRegionData = region));
      this.landfillService.getLandfillsByRegion(apiKey).subscribe((lfs) => {
        this.selectedLandfills = lfs;
        this.initializeLandfills();
      });
    }
  }

  private zoomToRegion(feKey: string): void {
    const bounds = this.regions[feKey];
    if (bounds) {
      this.map.fitBounds(bounds, { padding: 50, duration: 1200, bearing: 0 });
    }
  }

  // Place markers for currently selected landfills (call this after selectedLandfills updates if you want markers)
  private initializeLandfills(): void {
    // for (const landfill of this.selectedLandfills) {
    //   this.monitoringService.getLatestMonitoring(landfill.id).subscribe((monitoring) => {
    //     this.latestMonitorings[landfill.id] = monitoring;
    //   });
    // }
    for (const landfill of this.selectedLandfills) {
      const color = this.typeColors[landfill.category] || '#FF5B5B'; // fallback to red

      const marker = new mapboxgl.Marker({ color })
        .setLngLat([landfill.pointLon!, landfill.pointLat!])
        .setPopup(
          new mapboxgl.Popup({ closeButton: true, closeOnClick: false }).setHTML(
            `<strong style="font-size:16px;">${landfill.name ?? 'Unknown Landfill'}</strong>`
          )
        )
        .addTo(this.map);

      this.markers.push(marker);

      marker.getElement().addEventListener('click', () => {
        this.onLandfillClick(landfill);
      });
    }
  }

  toggleMapTypeDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  setMapType(type: 'satellite' | 'heatmap') {
    this.mapType = type;
    this.isDropdownOpen = false;

    // If you already have onMapTypeChange logic, call it here:
    const evt = { target: { value: type } } as unknown as Event;
    this.onMapTypeChange(evt);
  }

  onMapTypeChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.mapType = value as 'satellite' | 'heatmap';
    this.updateMapType();
  }

  private updateMapType(): void {
    if (this.markers) {
      this.markers.forEach((marker) => marker.remove());
      this.markers = [];
    }
    if (this.map.getLayer('landfill-heatmap')) {
      this.map.removeLayer('landfill-heatmap');
    }
    if (this.map.getSource('landfills')) {
      this.map.removeSource('landfills');
    }

    if (this.mapType === 'satellite') {
      this.map.setStyle('mapbox://styles/mapbox/satellite-streets-v12');
      this.map.once('style.load', () => {
        this.initializeLandfills();
      });
      return;
    }

    if (this.mapType === 'heatmap') {
      this.map.setStyle('mapbox://styles/mapbox/light-v10');
      const geojson: GeoJSON.FeatureCollection<GeoJSON.Point, { emission: number | undefined }> = {
        type: 'FeatureCollection',
        features: this.allLandfills.map((lf) => ({
          type: 'Feature',
          properties: {
            emission: lf.estimatedAreaM2 ?? 0,
          },
          geometry: {
            type: 'Point',
            coordinates: [lf.pointLon!, lf.pointLat!],
          },
        })),
      };

      this.map.once('style.load', () => {
        this.map.addSource('landfills', { type: 'geojson', data: geojson });
        this.map.addLayer({
          id: 'landfill-heatmap',
          type: 'heatmap',
          source: 'landfills',
          paint: {
            'heatmap-weight': [
              'interpolate',
              ['linear'],
              ['get', 'emission'],
              0,
              0,
              200,
              0.2,
              300,
              0.5,
              400,
              0.8,
              500,
              1,
              1300,
              1, // covers your extreme
            ],
            'heatmap-color': [
              'interpolate',
              ['linear'],
              ['heatmap-density'],
              0,
              'rgba(0,0,255,0)', // transparent blue
              0.2,
              'rgba(0,255,0,1)', // green
              0.4,
              'rgba(255,255,0,1)', // yellow
              0.6,
              'rgba(255,165,0,1)', // orange
              0.8,
              'rgba(255,0,0,1)', // red
              1,
              'rgba(255,0,0,1)', // red for max density
            ],
            'heatmap-radius': 35,
            'heatmap-opacity': 0.85,
          },
        });
      });
    }
  }

  getLandfillSizeString(size: number): string {
    if (size < 10000) {
      return 'Small';
    } else if (size < 50000) {
      return 'Medium';
    } else {
      return 'Large';
    }
  }

  getLandfillSizeInfo(size: number): { label: string; color: string } {
    if (size < 10000) {
      return { label: 'Small', color: '#4CAF50' }; // Green
    } else if (size < 50000) {
      return { label: 'Medium', color: '#FFC107' }; // Amber
    } else {
      return { label: 'Large', color: '#F44336' }; // Red
    }
  }
  getLandfillCategoryString(category: LandfillCategory | number): string {
    switch (category) {
      case LandfillCategory.Illegal:
        return 'illegal';
      case LandfillCategory.NonSanitary:
        return 'non-sanitary';
      case LandfillCategory.Sanitary:
        return 'sanitary';
      default:
        return 'unknown';
    }
  }
  onLandfillClick(lf: LandfillSite): void {
    this.selectedLandfill = lf;
    // this.monitoringService.getLatestMonitoring(lf.id).subscribe((m) => (this.latestMonitoring = m));

    // We don't have a bbox; center on point.
    this.landfillZoomIn(lf.pointLat!, lf.pointLon!, lf.pointLat!, lf.pointLon!);
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
      this.map.flyTo({ center: [seLng, seLat], zoom: 18, speed: 0.8, curve: 1.2 });
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
