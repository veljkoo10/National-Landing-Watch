import { Component, AfterViewInit, HostListener, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatFormField } from '@angular/material/form-field';
import { MatLabel } from '@angular/material/form-field';
import { MatSelect } from '@angular/material/select';
import { MatOption } from '@angular/material/select';
import { NgFor } from '@angular/common';
import mapboxgl from 'mapbox-gl';
import { SidebarCommunicationService } from '../../services/sidebar-communication-service';

@Component({
  selector: 'app-map',
  imports: [MatIconModule, MatFormField, MatLabel, MatSelect, MatOption, NgFor],
  standalone: true,
  templateUrl: './map-component.html',
  styleUrls: ['./map-component.css']
})
export class MapComponent implements AfterViewInit {

  map!: mapboxgl.Map;
  private sidebarService = inject(SidebarCommunicationService);

  private accessToken: string = 'pk.eyJ1IjoiYW5kamVsYW1yZGphIiwiYSI6ImNtZ2k1eGl0dTA1YnUybHF4ZDdmZnlqNTQifQ.B-W-2BJqVGziYH-15nCvIA';

  regions: Record<string, mapboxgl.LngLatBoundsLike> = {
    'Whole Serbia': [
      [18.7, 42.2],  // Southwest corner (lng, lat)
      [22.6, 46.2]   // Northeast corner
    ],
    'Vojvodina': [
      [18.6, 45.0],  
      [21.1, 46.2]  
    ],
    'Belgrade': [
      [20.2, 44.5],
      [20.7, 44.95]
    ],
    'Western Serbia': [
      [18.6, 43.4],
      [20.2, 44.6]
    ],
    'Šumadija i Pomoravlje': [
      [20.3, 43.6],
      [21.4, 44.3]
    ],
    'Eastern Serbia': [
      [21.4, 43.6],
      [22.6, 44.8]
    ],
    'Southern Serbia': [
      [20.6, 42.3],
      [22.4, 43.5]
    ]
  };

  ngAfterViewInit(): void {
    mapboxgl.accessToken = this.accessToken;
    this.map = new mapboxgl.Map({
      container: 'map',
      style: 'mapbox://styles/mapbox/satellite-streets-v12',
      center: [20.9, 43.9],
      zoom: 3
    });

    

    const landfills = [
      { name: 'Deponija Futog', coordX: 19.705, cordY: 45.258 },
      { name: 'Deponija Petrovaradin', coordX: 19.882, cordY: 45.241 },
    ];

    for (const landfill of landfills) {
      // Customize marker appearance
      const marker = new mapboxgl.Marker({
        color: '#FF5B5B', // Brighter red color
        scale: 0.8 // Slightly smaller marker
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

    // Add ambient lighting effect
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
  
  toggleSidebar() {
    this.sidebarService.toggleSidebar();
  }

  regionNames = Object.keys(this.regions);

  zoomToRegion(regionName: string) {
    const bounds = this.regions[regionName];
    if (bounds) {
      this.map.fitBounds(bounds, {
        padding: 50,
        duration: 2000,
        bearing: 0
      });
    }
  }

}
