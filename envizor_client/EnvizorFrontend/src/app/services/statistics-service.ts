import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';
import { 
  WasteByRegionDto,
  LandfillTypeDto,
  LandfillSite,
  MostImpactedRegionFullDto,
  GrowthRowDto,
  EmissionsPerCapitaDto
} from '../DTOs';

@Injectable({ providedIn: 'root' })
export class StatisticsService extends BaseService {
  private readonly statsUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.statistics}`;

  constructor(http: HttpClient) {
    super(http);
  }

  getTotalWasteByRegion(): Observable<WasteByRegionDto[]> {
    return this.http.get<WasteByRegionDto[]>(`${this.statsUrl}/total-waste-by-region`);
  }

  getLandfillTypes(): Observable<LandfillTypeDto[]> {
    return this.http.get<LandfillTypeDto[]>(`${this.statsUrl}/landfill-types`);
  }

  getCh4EmissionsOverTime(): Observable<{ years: number[]; ch4ByYear: number[] }> {
    return this.http.get<{ years: number[]; ch4ByYear: number[] }>(
      `${this.statsUrl}/ch4-over-time`
    );
  }

  getTop3LargestLandfills(): Observable<LandfillSite[]> {
    return this.http.get<LandfillSite[]>(`${this.statsUrl}/top3-largest`);
  }

  getMostImpactedRegion(): Observable<MostImpactedRegionFullDto> {
    return this.http.get<MostImpactedRegionFullDto>(`${this.statsUrl}/most-impacted`);
  }

  getLandfillGrowthOverYears(): Observable<GrowthRowDto[]> {
    return this.http.get<GrowthRowDto[]>(`${this.statsUrl}/landfill-growth`);
  }

  getEmissionsPerCapita(): Observable<EmissionsPerCapitaDto[]> {
    return this.http.get<EmissionsPerCapitaDto[]>(`${this.statsUrl}/emissions-per-capita`);
  }
}
