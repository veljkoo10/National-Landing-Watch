import { Component, AfterViewInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import mapboxgl from 'mapbox-gl';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NgFor } from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { TranslationService } from '../../services';
import { MatTooltip } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatTooltip, NgFor, FormsModule,TranslatePipe, RouterModule],
  templateUrl: './landing-component.html',
  styleUrls: ['./landing-component.css']
})
export class LandingComponent implements AfterViewInit {
  private accessToken: string = 'pk.eyJ1IjoiYW5kamVsYW1yZGphIiwiYSI6ImNtZ2k1eGl0dTA1YnUybHF4ZDdmZnlqNTQifQ.B-W-2BJqVGziYH-15nCvIA';
  selectedLang: string;
  isDarkMode = false;

  constructor(private router: Router, private translationService: TranslationService) {
    this.selectedLang = this.translationService.getLanguage();

    const storedTheme = localStorage.getItem('theme');
    if (storedTheme === 'dark') {
      this.isDarkMode = true;
      document.body.classList.add('dark-theme');
    }
  }

  ngAfterViewInit(): void {
    mapboxgl.accessToken = this.accessToken;
    const map = new mapboxgl.Map({
      container: 'globe',
      style: 'mapbox://styles/mapbox/satellite-v9',
      projection: 'globe',
      zoom: 1.5,
      center: [20, 30],
      interactive: false
    });

    // Slowly spin the globe
    let rotation = 0;
    function spinGlobe() {
      rotation += 0.05;
      map.setBearing(rotation);
      requestAnimationFrame(spinGlobe);
    }
    spinGlobe();
  }

  goToPage(path: string) {
    this.router.navigate([path], { replaceUrl: true });
  }

  changeLanguage(lang: string) {
    this.translationService.setLanguage(lang);
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
    const themeClass = 'dark-theme';
    if (this.isDarkMode) {
      document.body.classList.add(themeClass);
      localStorage.setItem('theme', 'dark');
    } else {
      document.body.classList.remove(themeClass);
      localStorage.setItem('theme', 'light');
    }
  }

  infoCards = [
  { icon: 'public', title: 'landing.info.card1.title', text: 'landing.info.card1.text' },
  { icon: 'science', title: 'landing.info.card2.title', text: 'landing.info.card2.text' },
  { icon: 'eco', title: 'landing.info.card3.title', text: 'landing.info.card3.text' },
  { icon: 'groups', title: 'landing.info.card4.title', text: 'landing.info.card4.text' },
  { icon: 'analytics', title: 'landing.info.card5.title', text: 'landing.info.card5.text' },
  { icon: 'lightbulb', title: 'landing.info.card6.title', text: 'landing.info.card6.text' }
];

  goToMap() {
    this.router.navigate(['/map']);
  }
}