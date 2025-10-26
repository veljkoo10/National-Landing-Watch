export interface MostImpactedRegionFullDto {
  region: string;
  totalCh4: number;
  totalCo2eq: number;
  ch4PerKm2: number;
  co2eqPerKm2: number;
  population: number;
  areaKm2: number;
}