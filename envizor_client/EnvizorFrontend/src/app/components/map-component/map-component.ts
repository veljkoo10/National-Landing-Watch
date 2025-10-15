import { Component, AfterViewInit, HostListener, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatFormField } from '@angular/material/form-field';
import { MatLabel } from '@angular/material/form-field';
import { MatSelect } from '@angular/material/select';
import { MatOption } from '@angular/material/select';
import { CommonModule, NgFor, NgIf } from '@angular/common';
import mapboxgl from 'mapbox-gl';
import { SidebarCommunicationService, RegionService } from '../../services';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { RegionDto } from '../../DTOs/region-DTO';

@Component({
  selector: 'app-map',
  imports: [
    MatIconModule,
    MatFormField,
    MatLabel,
    MatSelect,
    MatOption,
    NgFor,
    NgIf,
    CommonModule,
    TranslatePipe
  ],
  standalone: true,
  templateUrl: './map-component.html',
  styleUrls: ['./map-component.css']
})
export class MapComponent implements AfterViewInit {

  map!: mapboxgl.Map;
  private sidebarService = inject(SidebarCommunicationService);
  private regionService = inject(RegionService);

  private accessToken: string = 'pk.eyJ1IjoiYW5kamVsYW1yZGphIiwiYSI6ImNtZ2k1eGl0dTA1YnUybHF4ZDdmZnlqNTQifQ.B-W-2BJqVGziYH-15nCvIA';

  ngAfterViewInit(): void {
    mapboxgl.accessToken = this.accessToken;
    this.map = new mapboxgl.Map({
      container: 'map',
      style: 'mapbox://styles/mapbox/satellite-streets-v12',
      center: [20.9, 43.9],
      zoom: 2
    });

    const landfills = [
      { name: 'Deponija Futog', coordX: 19.705, cordY: 45.258 },
      { name: 'Deponija Petrovaradin', coordX: 19.882, cordY: 45.241 },
    ];

    for (const landfill of landfills) {
      const marker = new mapboxgl.Marker({
        color: '#FF5B5B',
        scale: 0.8
      })
        .setLngLat([landfill.coordX, landfill.cordY])
        .setPopup(
          new mapboxgl.Popup({
            closeButton: true,
            closeOnClick: false
          }).setHTML(`
            <strong style="display: block; margin-bottom: 8px; font-size: 16px;">
              ${landfill.name}
            </strong>
          `)
        )
        .addTo(this.map);
    }

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

  regions: Record<string, mapboxgl.LngLatBoundsLike> = {
    'wholeSerbia': [[18.7, 42.2], [22.6, 46.2]],
    'vojvodina': [[18.6, 45.0], [21.1, 46.2]],
    'belgrade': [[20.2, 44.5], [20.7, 44.95]],
    'westernSerbia': [[18.6, 43.4], [20.2, 44.6]],
    'sumadijaiPomoravlje': [[20.3, 43.6], [21.4, 44.3]],
    'easternSerbia': [[21.4, 43.6], [22.6, 44.8]],
    'southernSerbia': [[20.6, 42.3], [22.4, 43.5]]
  };

  regionKeyMap: Record<RegionDto['name'], string> = {
    'Whole Serbia': 'wholeSerbia',
    'Vojvodina': 'vojvodina',
    'Belgrade': 'belgrade',
    'Western Serbia': 'westernSerbia',
    'Sumadija i Pomoravlje': 'sumadijaiPomoravlje',
    'Eastern Serbia': 'easternSerbia',
    'Southern Serbia': 'southernSerbia'
  };

  regionKeys = Object.keys(this.regions);
  selectedRegionData: RegionDto | null = null;selectedLandfills: { id: number; regionKey: string; lat: number; lng: number; area: number }[] = [];
  hoveredCard: number | null = null;
  private isDragging = false;
  private lastMouseX = 0;

  private mockLandfills = [
    { id: 101, regionKey: 'vojvodina', lat: 45.258, lng: 19.705, area: 52000 },
    { id: 102, regionKey: 'vojvodina', lat: 45.241, lng: 19.882, area: 34000 },
    { id: 103, regionKey: 'vojvodina', lat: 42.241, lng: 39.882, area: 25000 },
    { id: 104, regionKey: 'vojvodina', lat: 41.241, lng: 29.882, area: 16000 },
    { id: 105, regionKey: 'vojvodina', lat: 13.241, lng: 45.882, area: 44000 },
    { id: 106, regionKey: 'vojvodina', lat: 23.241, lng: 23.882, area: 33000 },
    { id: 201, regionKey: 'belgrade', lat: 44.80, lng: 20.46, area: 95000 },
    { id: 301, regionKey: 'westernSerbia', lat: 43.85, lng: 19.84, area: 23000 },
    { id: 501, regionKey: 'easternSerbia', lat: 44.23, lng: 22.53, area: 61000 },
    { id: 601, regionKey: 'southernSerbia', lat: 42.9, lng: 21.9, area: 28000 },
    { id: 401, regionKey: 'sumadijaiPomoravlje', lat: 43.9, lng: 21.2, area: 35000 },
  ];

  @HostListener('window:resize')
  onResize() {
    this.map.resize();
  }

  toggleSidebar() {
    this.sidebarService.toggleSidebar();
  }

  zoomToRegion(regionKey: string) {
    const bounds = this.regions[regionKey];
    if (bounds) {
      this.map.fitBounds(bounds, {
        padding: 50,
        duration: 2000,
        bearing: 0
      });
    }

    this.loadRegionData(regionKey);
    this.selectedLandfills = this.mockLandfills.filter(lf => lf.regionKey === regionKey);
  }

  private loadRegionData(regionKey: string) {
    this.regionService.getRegionByName(regionKey).subscribe(region => {
      this.selectedRegionData = region;
    });
  }

  onLandfillClick(lf: any) {
    console.log(`Clicked landfill - ID: ${lf.id}`);
  }

  onCardsMouseMove(event: MouseEvent) {
    const container = (event.currentTarget as HTMLElement).closest('.landfill-cards-scroll') as HTMLElement;
    if (event.buttons === 1) {
      container.scrollLeft -= event.movementX;
    }
  }
}