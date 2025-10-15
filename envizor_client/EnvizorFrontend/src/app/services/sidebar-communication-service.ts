import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SidebarCommunicationService {
  // Signal for toggling sidebar
  sidebarToggleSignal = signal<boolean | undefined>(undefined);

  // method to trigger toggle
  toggleSidebar() {
    this.sidebarToggleSignal.set(!this.sidebarToggleSignal());
  }
}