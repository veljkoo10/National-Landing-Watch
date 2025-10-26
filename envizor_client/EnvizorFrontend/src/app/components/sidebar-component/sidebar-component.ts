import { Component, EventEmitter, Output, effect, inject, OnInit } from '@angular/core';
import { NavigationEnd, NavigationStart, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SidebarCommunicationService, TranslationService } from '../../services';
import { filter } from 'rxjs/operators';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { ViewChild } from '@angular/core';
import { MatSidenavContainer } from '@angular/material/sidenav';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatListModule,
    MatButtonModule,
    MatTooltipModule,
    CommonModule,
    FormsModule,
    TranslatePipe,
    RouterModule
  ],
  templateUrl: './sidebar-component.html',
  styleUrl: './sidebar-component.css'
})
export class SidebarComponent implements OnInit {
  @Output() sidebarToggled = new EventEmitter<boolean>();
  private sidebarService = inject(SidebarCommunicationService);
  private translationService = inject(TranslationService);
  private router = inject(Router);
  @ViewChild(MatSidenavContainer) sidenavContainer!: MatSidenavContainer;

  isSidebarOpen = false;
  showSidebar = true;
  isMapRoute = false;
  isLandingPage = false;
  isDarkMode = false;
  selectedLang = this.translationService.getLanguage();

  constructor() {
    // Sidebar signal reaction
    effect(() => {
      const toggle = this.sidebarService.sidebarToggleSignal();
      if (toggle !== undefined) this.toggleSidebar();
    });

    // Close sidebar before entering map page or if it is on landing page
    this.router.events
      .pipe(filter(event => event instanceof NavigationStart))
      .subscribe((event: NavigationStart) => {
        const url = event.url;
        this.isLandingPage = url.includes('/landing');
        this.isSidebarOpen = !(this.isLandingPage);
      });

    // Detect route (for ngClass)
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.isMapRoute = event.urlAfterRedirects.includes('/map');
      });

  }

  ngOnInit(): void {
    this.checkTheme();

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.checkTheme();
      });

    const currentUrl = this.router.url;
    this.isMapRoute = currentUrl.includes('/map');
  }

  private checkTheme() {
    this.isDarkMode = localStorage.getItem('theme') === 'dark';

    if (this.isDarkMode) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.remove('dark-theme');
    }
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  // Language change
  changeLanguage(lang: string) {
    this.translationService.setLanguage(lang);
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
    const body = document.body;

    if (this.isDarkMode) {
      body.classList.add('dark-theme');
      localStorage.setItem('theme', 'dark');
    } else {
      body.classList.remove('dark-theme');
      localStorage.setItem('theme', 'light');
    }

    setTimeout(() => {
      if (this.sidenavContainer) {
        this.sidenavContainer.updateContentMargins();
      }
    }, 100);

    window.location.reload();
  }
}
