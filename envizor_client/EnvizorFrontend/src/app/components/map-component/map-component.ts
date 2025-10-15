import { Component, AfterViewInit, HostListener, HostBinding, inject } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import mapboxgl from 'mapbox-gl';
import { MatIcon } from '@angular/material/icon';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { SidebarCommunicationService, RegionService, LandfillService } from '../../services';
import { RegionDto, LandfillDto, MonitoringDto } from '../../DTOs';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, NgFor, NgIf, TranslatePipe, MatIcon],
  templateUrl: './map-component.html',
  styleUrls: ['./map-component.css']
})
export class MapComponent implements AfterViewInit {
  // Mapbox
  map!: mapboxgl.Map;
  private readonly accessToken = 'pk.eyJ1IjoiYW5kamVsYW1yZGphIiwiYSI6ImNtZ2k1eGl0dTA1YnUybHF4ZDdmZnlqNTQifQ.B-W-2BJqVGziYH-15nCvIA';

  // Services
  private sidebarService = inject(SidebarCommunicationService);
  private regionService = inject(RegionService);
  private landfillService = inject(LandfillService);

  // UI state
  @HostBinding('class.sidebar-open') sidebarOpen = true;
  hoveredCard: number | null = null;

  // Data
  selectedRegionData: RegionDto | null = null;
  selectedLandfills: LandfillDto[] = [];
  selectedLandfill: LandfillDto | null = null;
  latestMonitoring: MonitoringDto | null = null;

  // Regions (for zoom)
  regions: Record<string, mapboxgl.LngLatBoundsLike> = {
    'wholeSerbia': [[18.7, 42.2], [22.6, 46.2]],
    'vojvodina': [[18.6, 45.0], [21.1, 46.2]],
    'belgrade': [[20.2, 44.5], [20.7, 44.95]],
    'westernSerbia': [[18.6, 43.4], [20.2, 44.6]],
    'sumadijaAndPomoravlje': [[20.3, 43.6], [21.4, 44.3]],
    'easternSerbia': [[21.4, 43.6], [22.6, 44.8]],
    'southernSerbia': [[20.6, 42.3], [22.4, 43.5]],
    'kosovoAndMetohija': [[20.3, 41.8], [22, 43.1]]
  };

  regionKeys = Object.keys(this.regions);

  getRegionKey(name: string): string {
    return name.toLowerCase().replace(/\s+/g, '');
  }

  tooltipText: string | null = null;
  tooltipPosition = { x: 0, y: 0 };

  ngAfterViewInit(): void {
    mapboxgl.accessToken = this.accessToken;
    this.initializeMap();
  }

  // Initialize Map
  private initializeMap(): void {
    this.map = new mapboxgl.Map({
      container: 'map',
      style: 'mapbox://styles/mapbox/satellite-streets-v12',
      center: [20.9, 43.9],
      zoom: 2
    });

    this.map.on('load', () => {
      this.map.setFog({
        'horizon-blend': 0.2,
        'color': '#242B4B',
        'high-color': '#161B36',
        'space-color': '#0B1026',
        'star-intensity': 0.6
      });
    });
  }

  // Window resize listener
  @HostListener('window:resize')
  onResize() {
    this.map?.resize();
  }

  // Sidebar toggle
  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
    this.sidebarService.toggleSidebar();
  }

  // Region selection handler
  onRegionSelect(event: Event): void {
    const regionKey = (event.target as HTMLSelectElement).value;
    this.zoomToRegion(regionKey);
  }

  // Zoom and load region details
  private zoomToRegion(regionKey: string): void {
    const bounds = this.regions[regionKey];
    if (bounds) {
      this.map.fitBounds(bounds, { padding: 50, duration: 2000, bearing: 0 });
    }

    this.selectedLandfill = null;
    this.latestMonitoring = null;
    this.selectedRegionData = null;

    // Future: Replace with backend call
    this.regionService.getRegionByName(regionKey).subscribe(region => (this.selectedRegionData = region));
    this.landfillService.getLandfillsByRegion(regionKey).subscribe(lfs => (this.selectedLandfills = lfs));
  }

  // Landfill selection
  onLandfillClick(lf: LandfillDto): void {
    this.selectedLandfill = lf;
    this.landfillService.getLatestMonitoring(lf.id).subscribe(m => (this.latestMonitoring = m));
  }

  // Card scrolling
  onCardsMouseMove(event: MouseEvent): void {
    const container = (event.currentTarget as HTMLElement).closest('.landfill-cards-scroll') as HTMLElement;
    if (event.buttons === 1) container.scrollLeft -= event.movementX;
  }

  // Tooltip handling
  showTooltip(event: MouseEvent, text: string): void {
    this.tooltipText = text;
    const rect = (event.target as HTMLElement).getBoundingClientRect();
    this.tooltipPosition = { x: rect.left + rect.width / 2, y: rect.top - 12 };
  }

  hideTooltip(): void {
    this.tooltipText = null;
  }
}