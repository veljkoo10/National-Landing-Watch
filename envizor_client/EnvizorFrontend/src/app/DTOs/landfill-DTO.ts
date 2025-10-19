export interface LandfillDto {
  id: number;
  name?: string;
  regionKey: string;
  latitude: number;
  longitude: number;
  type: 'sanitary' | 'unsanitary' | 'wild';
  size: 'small' | 'medium' | 'large';
  yearCreated: number;
  areaM2?: number;
}
