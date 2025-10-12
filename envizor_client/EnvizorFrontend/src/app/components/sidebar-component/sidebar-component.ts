import { Component, EventEmitter, Output, effect, inject } from '@angular/core';
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
export class SidebarComponent {
  @Output() sidebarToggled = new EventEmitter<boolean>();
  private sidebarService = inject(SidebarCommunicationService);
  private translationService = inject(TranslationService);

  isSidebarOpen = false;
  isMapRoute = false;
  isLandingPage = false;
  isDarkMode = false;
  selectedLang = this.translationService.getLanguage();

  constructor(private router: Router) {
    // Sidebar signal reaction
    effect(() => {
      const toggle = this.sidebarService.sidebarToggleSignal();
      if (toggle !== undefined) this.toggleSidebar();
    });

    // Close sidebar before entering map page or if it is on landing page
    this.router.events
      .pipe(filter(event => event instanceof NavigationStart))
      .subscribe((event: NavigationStart) => {
        if (event.url.includes('/map') || event.url.includes('/landing')) {
        this.isSidebarOpen = false;
        } else {
          this.isSidebarOpen = true;
        }
      });

    // Detect route (for ngClass)
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.isMapRoute = event.urlAfterRedirects.includes('/map');
      });

    // Initialize theme from localStorage
    const storedTheme = localStorage.getItem('theme');
    if (storedTheme === 'dark') {
      this.isDarkMode = true;
      document.body.classList.add('dark-theme');
    }
  }

  ngOnInit(): void {
    // Check initial route when component loads
    const currentUrl = this.router.url;
    this.isMapRoute = currentUrl.includes('/map');
  }

  navigateToMap() {
    this.isSidebarOpen = false;
    setTimeout(() => {
        this.router.navigate(['/map']);
      }, 400);
  }

  toggleSidebar() {
    this.isSidebarOpen = !this.isSidebarOpen;
  }

  // Language change
  changeLanguage(lang: string) {
    this.translationService.setLanguage(lang);
  }

  // Theme toggle
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
}