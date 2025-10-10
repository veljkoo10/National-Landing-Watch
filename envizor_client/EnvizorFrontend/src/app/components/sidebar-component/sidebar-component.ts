  import { Component, EventEmitter, Output, effect, inject } from '@angular/core';
  import { Router, RouterModule } from '@angular/router';
  import { MatSidenavModule } from '@angular/material/sidenav';
  import { MatToolbarModule } from '@angular/material/toolbar';
  import { MatIconModule } from '@angular/material/icon';
  import { MatListModule } from '@angular/material/list';
  import { MatButtonModule } from '@angular/material/button';
  import { SidebarCommunicationService } from '../../services/sidebar-communication-service';

  @Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [MatSidenavModule, MatToolbarModule, MatIconModule, MatListModule, MatButtonModule, RouterModule],
    templateUrl: './sidebar-component.html',
    styleUrl: './sidebar-component.css'
  })
  export class SidebarComponent {
    @Output() sidebarToggled = new EventEmitter<boolean>();
    private sidebarService = inject(SidebarCommunicationService);

    isSidebarOpen = false;

    constructor(private router: Router) {
      effect(() => {
        const toggle = this.sidebarService.sidebarToggleSignal();
        if (toggle !== undefined) this.toggleSidebar();
      });
    }

    toggleSidebar() {
      this.isSidebarOpen = !this.isSidebarOpen;
      this.sidebarToggled.emit(this.isSidebarOpen); // emit toggle event
    }

    navigateTo(route: string) {
      this.router.navigate([route]);
      if (window.innerWidth < 768) {
        this.isSidebarOpen = false;
        this.sidebarToggled.emit(this.isSidebarOpen);
      }
    }
  }
