import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';

export type WasteByRegionDto = { name: string; totalWaste: number };
export type LandfillTypeDto = { name: string; count: number };
export type Ch4OverTimeDto = { years: number[]; ch4ByYear: number[] };
export type TopLandfillDto = {
  name: string;
  region: string;
  areaM2?: number;
  yearCreated?: number;
};
export interface MostImpactedRegionFullDto {
  region: string;
  totalCh4: number;
  totalCo2eq: number;
  ch4PerKm2: number;
  co2eqPerKm2: number;
  population: number;
  areaKm2: number;
}
export type GrowthRowDto = { year: number; landfillCount: number };
export type EmissionsPerCapitaDto = { region: string; ch4PerCapita: number };
export interface TableRow {
  landfillName: string;
  regionName: string;
  year: number;
  volumeM3: number;
  wasteTons: number;
  ch4Tons: number;
  co2eqTons: number;
}

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

  getTop3LargestLandfills(): Observable<TopLandfillDto[]> {
    return this.http.get<TopLandfillDto[]>(`${this.statsUrl}/top3-largest`);
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

  getLandfillStats(): Observable<TableRow[]> {
    return this.http.get<TableRow[]>(`${this.statsUrl}/landfill-stats`);
  }
}
