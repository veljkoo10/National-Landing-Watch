import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';
import { LandfillDto } from '../DTOs';
import { LandfillSite } from '../DTOs/landfillSiteAll-DTO';

@Injectable({ providedIn: 'root' })
export class LandfillService extends BaseService {
  private readonly landfillUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.landfills}`;

  constructor(http: HttpClient) {
    super(http);
  }

  getAllLandfills(): Observable<LandfillSite[]> {
    return this.get<LandfillSite[]>(this.landfillUrl);
  }

  // getLandfillById(id: number): Observable<LandfillDto> {
  //   return this.get<LandfillDto>(`${this.landfillUrl}/${id}`);
  // }

  getLandfillsByRegion(regionKey: string): Observable<LandfillSite[]> {
    // backend route: GET /api/landfills/region/{key}
    return this.get<LandfillSite[]>(`${this.landfillUrl}/region/${encodeURIComponent(regionKey)}`);
  }

  getLandfillById(id: number): Observable<LandfillSite> {
    return this.get<LandfillSite>(`${this.landfillUrl}/${id}`);
  }
}
