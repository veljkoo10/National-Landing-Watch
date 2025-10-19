import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { API_CONFIG } from './api-config';
import { RegionDto } from '../DTOs';
import { BaseService } from './base-service';

@Injectable({ providedIn: 'root' })
export class RegionService extends BaseService {
  private readonly regionUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.regions}`;
  constructor(http: HttpClient) {
    super(http);
  }

  getRegionByName(name: string): Observable<RegionDto> {
    // Component passes a human display name (e.g. "Beograd"); we encode here.
    return this.get<RegionDto>(`${this.regionUrl}/name/${encodeURIComponent(name)}`);
  }

  getAllRegions(): Observable<RegionDto[]> {
    return this.get<RegionDto[]>(this.regionUrl);
  }

  getRegionById(id: number): Observable<RegionDto> {
    return this.get<RegionDto>(`${this.regionUrl}/${id}`);
  }
}
