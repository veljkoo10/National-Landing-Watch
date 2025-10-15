import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { API_CONFIG } from './api-config';
import { BaseService } from './base-service';
import { LandfillDto, MonitoringDto } from '../DTOs';

@Injectable({ providedIn: 'root' })
export class LandfillService extends BaseService {
  private readonly landfillUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.landfills}`;
  private readonly monitoringUrl = `${API_CONFIG.baseUrl}${API_CONFIG.endpoints.monitorings}`;

  constructor(http: HttpClient) {
    super(http);
  }

  getLandfillsByRegion(regionKey: string): Observable<LandfillDto[]> {
    return of(this.mockLandfills.filter(l => l.regionKey === regionKey));
  }

  getLatestMonitoring(landfillId: number): Observable<MonitoringDto | null> {
    const monitorings = this.mockMonitorings.filter(m => m.landfillId === landfillId);
    const latest = monitorings.length ? monitorings.sort((a, b) => b.year - a.year)[0] : null;
    return of(latest);
  }

  getAllLandfills(): Observable<LandfillDto[]> {
    return of(this.mockLandfills);
  }

  private mockLandfills: LandfillDto[] = [
    { id: 101, regionKey: 'vojvodina', latitude: 45.258, longitude: 19.705, type: 'sanitary', size: 'large', yearCreated: 2001 },
    { id: 102, regionKey: 'vojvodina', latitude: 45.241, longitude: 19.882, type: 'unsanitary', size: 'medium', yearCreated: 1998 },
    { id: 103, regionKey: 'vojvodina', latitude: 42.241, longitude: 39.882, type: 'wild', size: 'small', yearCreated: 2011 },
    { id: 104, regionKey: 'vojvodina', latitude: 41.241, longitude: 29.882, type: 'sanitary', size: 'small', yearCreated: 2003 },
    { id: 105, regionKey: 'vojvodina', latitude: 13.241, longitude: 45.882, type: 'unsanitary', size: 'medium', yearCreated: 2008 },
    { id: 106, regionKey: 'vojvodina', latitude: 23.241, longitude: 23.882, type: 'wild', size: 'medium', yearCreated: 2015 },
    { id: 201, regionKey: 'belgrade', latitude: 44.8, longitude: 20.46, type: 'sanitary', size: 'large', yearCreated: 2002 },
    { id: 301, regionKey: 'westernSerbia', latitude: 43.85, longitude: 19.84, type: 'unsanitary', size: 'small', yearCreated: 2007 },
    { id: 401, regionKey: 'sumadijaAndPomoravlje', latitude: 43.9, longitude: 21.2, type: 'wild', size: 'medium', yearCreated: 2012 },
    { id: 501, regionKey: 'easternSerbia', latitude: 44.23, longitude: 22.53, type: 'sanitary', size: 'large', yearCreated: 2005 },
    { id: 601, regionKey: 'southernSerbia', latitude: 42.9, longitude: 21.9, type: 'unsanitary', size: 'small', yearCreated: 2009 },
    { id: 701, regionKey: 'kosovoAndMetohija', latitude: 43.7, longitude: 23.5, type: 'wild', size: 'small', yearCreated: 2010 }
  ];

  private mockMonitorings: MonitoringDto[] = [
    { id: 1, landfillId: 101, year: 2023, areaM2: 52000, volumeM3: 470000, wasteTons: 83000, ch4Tons: 4300, co2Tons: 10500 },
    { id: 2, landfillId: 102, year: 2023, areaM2: 34000, volumeM3: 290000, wasteTons: 61000, ch4Tons: 3100, co2Tons: 8500 },
    { id: 3, landfillId: 103, year: 2023, areaM2: 25000, volumeM3: 210000, wasteTons: 41000, ch4Tons: 2300, co2Tons: 6700 },
    { id: 4, landfillId: 104, year: 2023, areaM2: 16000, volumeM3: 120000, wasteTons: 22000, ch4Tons: 1300, co2Tons: 4200 },
    { id: 5, landfillId: 105, year: 2023, areaM2: 44000, volumeM3: 390000, wasteTons: 72000, ch4Tons: 3400, co2Tons: 9200 },
    { id: 6, landfillId: 106, year: 2023, areaM2: 33000, volumeM3: 270000, wasteTons: 54000, ch4Tons: 2900, co2Tons: 7800 },
    { id: 7, landfillId: 201, year: 2023, areaM2: 95000, volumeM3: 880000, wasteTons: 110000, ch4Tons: 5100, co2Tons: 11200 },
    { id: 8, landfillId: 301, year: 2023, areaM2: 23000, volumeM3: 190000, wasteTons: 37000, ch4Tons: 2200, co2Tons: 6000 },
    { id: 9, landfillId: 401, year: 2023, areaM2: 35000, volumeM3: 310000, wasteTons: 56000, ch4Tons: 3100, co2Tons: 8500 },
    { id: 10, landfillId: 501, year: 2023, areaM2: 61000, volumeM3: 560000, wasteTons: 93000, ch4Tons: 4500, co2Tons: 10900 },
    { id: 11, landfillId: 601, year: 2023, areaM2: 28000, volumeM3: 240000, wasteTons: 46000, ch4Tons: 2500, co2Tons: 6900 },
    { id: 12, landfillId: 701, year: 2023, areaM2: 9000, volumeM3: 180000, wasteTons: 36000, ch4Tons: 1900, co2Tons: 4900 }
  ];

  // Landfills
  // getAllLandfills(): Observable<LandfillDto[]> {
  //   return this.get<LandfillDto[]>(this.landfillUrl);
  // }

  getLandfillById(id: number): Observable<LandfillDto> {
    return this.get<LandfillDto>(`${this.landfillUrl}/${id}`);
  }

  // getLandfillsByRegion(regionKey: string): Observable<LandfillDto[]> {
  //   return this.get<LandfillDto[]>(`${this.landfillUrl}/region/${regionKey}`);
  // }

  // Monitorings
  getMonitoringsByLandfill(landfillId: number): Observable<MonitoringDto[]> {
    return this.get<MonitoringDto[]>(`${this.monitoringUrl}/landfill/${landfillId}`);
  }

  // getLatestMonitoring(landfillId: number): Observable<MonitoringDto> {
  //   return this.get<MonitoringDto>(`${this.monitoringUrl}/landfill/${landfillId}/latest`);
  // }
}