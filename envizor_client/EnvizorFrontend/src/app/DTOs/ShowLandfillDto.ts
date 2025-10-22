export interface ShowLandfillDto {
  id: number; // Unique identifier
  name?: string; // Known landfill name (if available)

  regionKey: string; // e.g. "Vojvodina", "Belgrade"
  latitude: number; // Center latitude
  longitude: number; // Center longitude

  type: string; // sanitary | unsanitary | wild
  size: string; // small | medium | large
  yearCreated: number;

  areaM2?: number; // Surface area if available
}
