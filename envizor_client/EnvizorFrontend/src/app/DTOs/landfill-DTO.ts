export interface LandfillDto {
  id: number;
  regionKey: string;
  latitude: number;
  longitude: number;
  type: 'sanitary' | 'unsanitary' | 'wild';
  size: 'small' | 'medium' | 'large';
  yearCreated: number;
}