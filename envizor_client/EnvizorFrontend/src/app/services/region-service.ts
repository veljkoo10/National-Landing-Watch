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

  // Mock data for now — connect to API later
  private regions: RegionDto[] = [
    {
      id: 1, name: 'Vojvodina', population: 1900000, areaKm2: 21506,
      ch4Tons: 4300, co2Tons: 102000, landfillCount: 15, pollutionLevel: 'medium',
      totalWaste: 210000
    },
    {
      id: 2, name: 'Belgrade', population: 1675000, areaKm2: 3226,
      ch4Tons: 5100, co2Tons: 112000, landfillCount: 8, pollutionLevel: 'high',
      totalWaste: 250000
    },
    {
      id: 3, name: 'Western Serbia', population: 1200000, areaKm2: 26000,
      ch4Tons: 2800, co2Tons: 72000, landfillCount: 10, pollutionLevel: 'medium',
      totalWaste: 160000
    },
    {
      id: 4, name: 'Sumadija And Pomoravlje', population: 1000000, areaKm2: 13000,
      ch4Tons: 2000, co2Tons: 60000, landfillCount: 7, pollutionLevel: 'low',
      totalWaste: 95000
    },
    {
      id: 5, name: 'Kosovo And Metohija', population: 900000, areaKm2: 9000,
      ch4Tons: 2000, co2Tons: 60000, landfillCount: 7, pollutionLevel: 'medium',
      totalWaste: 95000
    }
  ];

  getRegionByName(name: string): Observable<RegionDto | null> {
    const normalize = (str: string) => str.toLowerCase().replace(/\s+/g, '');
    const region = this.regions.find(r => normalize(r.name) === normalize(name)) ?? null;
    return of(region);
  }

  getAllRegions(): Observable<RegionDto[]> {
    return this.get<RegionDto[]>(this.regionUrl);
  }

  getRegionById(id: number): Observable<RegionDto> {
    return this.get<RegionDto>(`${this.regionUrl}/${id}`);
  }

  getRegionByKey(key: string): Observable<RegionDto> {
    return this.get<RegionDto>(`${this.regionUrl}/key/${key}`);
  }
}