import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';
import { MonitoringDto } from '../DTOs';

@Injectable({ providedIn: 'root' })
export class MonitoringService extends BaseService {
  private readonly monitoringUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.monitorings}`;

  constructor(http: HttpClient) {
    super(http);
  }

  getMonitoringsByLandfill(landfillId: number): Observable<MonitoringDto[]> {
    return this.get<MonitoringDto[]>(`${this.monitoringUrl}/landfill/${landfillId}`);
  }

  getLatestMonitoring(landfillId: number): Observable<MonitoringDto> {
    return this.get<MonitoringDto>(`${this.monitoringUrl}/landfill/${landfillId}/latest`);
  }
}
