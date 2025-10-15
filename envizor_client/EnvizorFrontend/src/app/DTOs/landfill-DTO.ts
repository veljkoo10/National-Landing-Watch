
export interface LandfillDto {
  id: number;
  regionId: number;
  latitude: number;
  longitude: number;
  type: 'sanitary' | 'unsanitary' | 'wild';
  size: 'small' | 'medium' | 'large';
  yearCreated: number;
}