import { TestBed } from '@angular/core/testing';

import { SidebarCommunicationService } from './sidebar-communication-service';

describe('SidebarCommunicationService', () => {
  let service: SidebarCommunicationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SidebarCommunicationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
