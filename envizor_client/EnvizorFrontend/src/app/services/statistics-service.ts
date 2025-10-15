import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';

@Injectable({ providedIn: 'root' })
export class StatisticsService extends BaseService {
  private readonly statsUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.statistics}`;

  constructor(http: HttpClient) {
    super(http);
  }

  getTotalWasteByRegion(): Observable<any> {
    return this.get(`${this.statsUrl}/total-waste-by-region`);
  }

  getLandfillTypes(): Observable<any> {
    return this.get(`${this.statsUrl}/landfill-types`);
  }

  getCh4EmissionsOverTime(): Observable<any> {
    return this.get(`${this.statsUrl}/ch4-over-time`);
  }

  getTop3LargestLandfills(): Observable<any> {
    return this.get(`${this.statsUrl}/top3-largest`);
  }

  getMostImpactedRegion(): Observable<any> {
    return this.get(`${this.statsUrl}/most-impacted`);
  }

  getLandfillGrowthOverYears(): Observable<any> {
    return this.get(`${this.statsUrl}/landfill-growth`);
  }

  getEmissionsPerCapita(): Observable<any> {
    return this.get(`${this.statsUrl}/emissions-per-capita`);
  }
}