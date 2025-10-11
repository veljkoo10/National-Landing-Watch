import { Routes } from '@angular/router';
import { 
  MapComponent, 
  FaqComponent, 
  AboutComponent, 
  StatisticsComponent, 
  BlogComponent 
} from './components';

export const routes: Routes = [
  { path: '', redirectTo: '/map', pathMatch: 'full' },
  { path: 'map', component: MapComponent },
  { path: 'faq', component: FaqComponent },
  { path: 'about', component: AboutComponent },
  { path: 'statistics', component: StatisticsComponent },
  { path: 'blog', component: BlogComponent },
  { path: '**', redirectTo: '/map' }
];