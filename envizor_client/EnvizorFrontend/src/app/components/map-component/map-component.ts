import { Component, AfterViewInit, HostListener, inject, HostBinding } from '@angular/core';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import mapboxgl from 'mapbox-gl';
import { SidebarCommunicationService, RegionService } from '../../services';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { RegionDto } from '../../DTOs/region-DTO';
import { LandfillDto, MonitoringDto } from '../../DTOs';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-map',
  imports: [NgFor, NgIf, CommonModule, TranslatePipe, MatIcon],
  standalone: true,
  templateUrl: './map-component.html',
  styleUrls: ['./map-component.css']
})
export class MapComponent implements AfterViewInit {
  map!: mapboxgl.Map;
  private sidebarService = inject(SidebarCommunicationService);
  private regionService = inject(RegionService);
  private accessToken: string = 'pk.eyJ1IjoiYW5kamVsYW1yZGphIiwiYSI6ImNtZ2k1eGl0dTA1YnUybHF4ZDdmZnlqNTQifQ.B-W-2BJqVGziYH-15nCvIA';

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

  regionKeyMap: Record<RegionDto['name'], string> = {
    'Whole Serbia': 'wholeSerbia',
    'Vojvodina': 'vojvodina',
    'Belgrade': 'belgrade',
    'Western Serbia': 'westernSerbia',
    'Sumadija And Pomoravlje': 'sumadijaAndPomoravlje',
    'Eastern Serbia': 'easternSerbia',
    'Southern Serbia': 'southernSerbia',
    'Kosovo And Metohija': 'kosovoAndMetohija'
  };

  regionKeys = Object.keys(this.regions);

  selectedRegionData: RegionDto | null = null;
  selectedLandfills: LandfillDto[] = [];
  selectedLandfill: LandfillDto | null = null;
  latestMonitoring: MonitoringDto | null = null;
  hoveredCard: number | null = null;

  private mockLandfills: LandfillDto[] = [
    { id: 101, regionKey: 'vojvodina', latitude: 45.258, longitude: 19.705, type: 'sanitary', size: 'large', yearCreated: 2001 },
    { id: 102, regionKey: 'vojvodina', latitude: 45.241, longitude: 19.882, type: 'unsanitary', size: 'medium', yearCreated: 1998 },
    { id: 103, regionKey: 'vojvodina', latitude: 42.241, longitude: 39.882, type: 'wild', size: 'small', yearCreated: 2011 },
    { id: 104, regionKey: 'vojvodina', latitude: 41.241, longitude: 29.882, type: 'sanitary', size: 'small', yearCreated: 2003 },
    { id: 105, regionKey: 'vojvodina', latitude: 13.241, longitude: 45.882, type: 'unsanitary', size: 'medium', yearCreated: 2008 },
    { id: 106, regionKey: 'vojvodina', latitude: 23.241, longitude: 23.882, type: 'wild', size: 'medium', yearCreated: 2015 },
    { id: 201, regionKey: 'belgrade', latitude: 44.8, longitude: 20.46, type: 'sanitary', size: 'large', yearCreated: 2002 },
    { id: 301, regionKey: 'westernSerbia', latitude: 43.85, longitude: 19.84, type: 'unsanitary', size: 'small', yearCreated: 2007 },
    { id: 401, regionKey: 'sumadijaAndPomoravlje', latitude: 43.9, longitude: 21.2, type: 'wild', size: 'medium', yearCreated: 2012 },
    { id: 501, regionKey: 'easternSerbia', latitude: 44.23, longitude: 22.53, type: 'sanitary', size: 'large', yearCreated: 2005 },
    { id: 601, regionKey: 'southernSerbia', latitude: 42.9, longitude: 21.9, type: 'unsanitary', size: 'small', yearCreated: 2009 },
    { id: 701, regionKey: 'kosovoAndMetohija', latitude: 43.7, longitude: 23.5, type: 'wild', size: 'small', yearCreated: 2010 }
  ];

  private mockMonitorings: MonitoringDto[] = [
    { id: 1, landfillId: 101, year: 2023, areaM2: 52000, volumeM3: 470000, wasteTons: 83000, ch4Tons: 4300, co2Tons: 10500 },
    { id: 2, landfillId: 102, year: 2023, areaM2: 34000, volumeM3: 290000, wasteTons: 61000, ch4Tons: 3100, co2Tons: 8500 },
    { id: 3, landfillId: 103, year: 2023, areaM2: 25000, volumeM3: 210000, wasteTons: 41000, ch4Tons: 2300, co2Tons: 6700 },
    { id: 4, landfillId: 104, year: 2023, areaM2: 16000, volumeM3: 120000, wasteTons: 22000, ch4Tons: 1300, co2Tons: 4200 },
    { id: 5, landfillId: 105, year: 2023, areaM2: 44000, volumeM3: 390000, wasteTons: 72000, ch4Tons: 3400, co2Tons: 9200 },
    { id: 6, landfillId: 106, year: 2023, areaM2: 33000, volumeM3: 270000, wasteTons: 54000, ch4Tons: 2900, co2Tons: 7800 },
    { id: 7, landfillId: 201, year: 2023, areaM2: 95000, volumeM3: 880000, wasteTons: 110000, ch4Tons: 5100, co2Tons: 11200 },
    { id: 8, landfillId: 301, year: 2023, areaM2: 23000, volumeM3: 190000, wasteTons: 37000, ch4Tons: 2200, co2Tons: 6000 },
    { id: 9, landfillId: 401, year: 2023, areaM2: 35000, volumeM3: 310000, wasteTons: 56000, ch4Tons: 3100, co2Tons: 8500 },
    { id: 10, landfillId: 501, year: 2023, areaM2: 61000, volumeM3: 560000, wasteTons: 93000, ch4Tons: 4500, co2Tons: 10900 },
    { id: 11, landfillId: 601, year: 2023, areaM2: 28000, volumeM3: 240000, wasteTons: 46000, ch4Tons: 2500, co2Tons: 6900 },
    { id: 12, landfillId: 701, year: 2023, areaM2: 9000, volumeM3: 180000, wasteTons: 36000, ch4Tons: 1900, co2Tons: 4900 }
  ];

  ngAfterViewInit(): void {
    mapboxgl.accessToken = this.accessToken;
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

  @HostListener('window:resize')
  onResize() {
    this.map.resize();
  }

  @HostBinding('class.sidebar-open') sidebarOpen = true;
  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
    this.sidebarService.toggleSidebar();
  }

  onRegionSelect(event: Event) {
    const regionKey = (event.target as HTMLSelectElement).value;
    this.zoomToRegion(regionKey);
  }

  zoomToRegion(regionKey: string) {
    const bounds = this.regions[regionKey];
    if (bounds) this.map.fitBounds(bounds, { padding: 50, duration: 2000, bearing: 0 });

    this.selectedRegionData = null;
    this.selectedLandfill = null;
    this.latestMonitoring = null;

    this.loadRegionData(regionKey);
    this.selectedLandfills = this.mockLandfills.filter(lf => lf.regionKey === regionKey);
  }

  private loadRegionData(regionKey: string) {
    this.regionService.getRegionByName(regionKey).subscribe(region => {
      this.selectedRegionData = region;
    });
  }

  onLandfillClick(lf: LandfillDto) {
    this.selectedLandfill = lf;
    const monitorings = this.mockMonitorings.filter(m => m.landfillId === lf.id);
    if (monitorings.length) {
      this.latestMonitoring = monitorings.sort((a, b) => b.year - a.year)[0];
    }
  }

  onCardsMouseMove(event: MouseEvent) {
    const container = (event.currentTarget as HTMLElement).closest('.landfill-cards-scroll') as HTMLElement;
    if (event.buttons === 1) container.scrollLeft -= event.movementX;
  }

  tooltipText: string | null = null;
  tooltipPosition = { x: 0, y: 0 };

  showTooltip(event: MouseEvent, text: string) {
    this.tooltipText = text;
    const rect = (event.target as HTMLElement).getBoundingClientRect();
    this.tooltipPosition = { x: rect.left + rect.width / 2, y: rect.top - 12 };
  }

  hideTooltip() {
    this.tooltipText = null;
  }
}