import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';
import { LandfillDto } from '../DTOs';

@Injectable({ providedIn: 'root' })
export class LandfillService extends BaseService {
  private readonly landfillUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.landfills}`;

  constructor(http: HttpClient) {
    super(http);
  }

  getAllLandfills(): Observable<LandfillDto[]> {
    return this.get<LandfillDto[]>(this.landfillUrl);
  }

  getLandfillById(id: number): Observable<LandfillDto> {
    return this.get<LandfillDto>(`${this.landfillUrl}/${id}`);
  }

  getLandfillsByRegion(regionKey: string): Observable<LandfillDto[]> {
    // backend route: GET /api/landfills/region/{key}
    return this.get<LandfillDto[]>(`${this.landfillUrl}/region/${encodeURIComponent(regionKey)}`);
  }
}
