import { LandfillCategory } from './Enums/LandfillCategory';
import { SerbianRegion } from './Enums/SerbianRegion';

export interface LandfillSite {
  id: number;
  name?: string;
  category: LandfillCategory;
  regionTag?: string;
  region?: SerbianRegion;
  pointLat?: number;
  pointLon?: number;
  boundaryGeoJson?: string;
  estimatedAreaM2?: number;
  estimatedVolumeM3?: number;
  estimatedDepth?: number;
  estimatedDensity?: number;
  estimatedMSW?: number;
  mcf?: number;
  estimatedCH4Tons?: number;
  estimatedCO2eTons?: number;
  startYear?: number;
  createdAt: Date;
  updatedAt: Date;
  // detections?: LandfillDetection[];
}