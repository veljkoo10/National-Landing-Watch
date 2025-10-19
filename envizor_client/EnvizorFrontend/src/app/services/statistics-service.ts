import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';

export interface WasteByRegion {
  name: string;
  totalWaste: number;
}
export interface LandfillType {
  name: string;
  count: number;
}
export interface Ch4OverTime {
  years: number[];
  ch4ByYear: number[];
}
export interface TopLandfill {
  name: string;
  region: string;
  areaM2?: number;
  yearCreated?: number;
}
export interface MostImpacted {
  region: string;
  totalCh4: number;
}
export interface GrowthRow {
  year: number;
  landfillCount: number;
}

@Injectable({ providedIn: 'root' })
export class StatisticsService extends BaseService {
  private readonly statsUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.statistics}`;

  constructor(http: HttpClient) {
    super(http);
  }

  getTotalWasteByRegion(): Observable<WasteByRegion[]> {
    return this.get<WasteByRegion[]>(`${this.statsUrl}/total-waste-by-region`);
  }
  getLandfillTypes(): Observable<LandfillType[]> {
    return this.get<LandfillType[]>(`${this.statsUrl}/landfill-types`);
  }
  getCh4EmissionsOverTime(): Observable<Ch4OverTime> {
    return this.get<Ch4OverTime>(`${this.statsUrl}/ch4-over-time`);
  }
  getTop3LargestLandfills(): Observable<TopLandfill[]> {
    return this.get<TopLandfill[]>(`${this.statsUrl}/top3-largest`);
  }
  getMostImpactedRegion(): Observable<MostImpacted> {
    return this.get<MostImpacted>(`${this.statsUrl}/most-impacted`);
  }
  getLandfillGrowthOverYears(): Observable<GrowthRow[]> {
    return this.get<GrowthRow[]>(`${this.statsUrl}/landfill-growth`);
  }
}
