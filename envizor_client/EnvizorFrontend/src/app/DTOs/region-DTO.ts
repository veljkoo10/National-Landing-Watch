
export interface RegionDto {
  id: number;
  name: string;
  population: number;
  areaKm2: number;
  ch4Tons: number;
  co2Tons: number;
  landfillCount: number;
  pollutionLevel: 'low' | 'medium' | 'high';
  totalWaste: number;
}