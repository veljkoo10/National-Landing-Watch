import { Routes } from '@angular/router';
import { 
  MapComponent, 
  FaqComponent, 
  AboutComponent, 
  StatisticsComponent, 
  BlogComponent,
  LandingComponent
} from './components';

export const routes: Routes = [
  { path: '', redirectTo: '/landing', pathMatch: 'full' },
  { path: 'map', component: MapComponent },
  { path: 'faq', component: FaqComponent },
  { path: 'about', component: AboutComponent },
  { path: 'statistics', component: StatisticsComponent },
  { path: 'blog', component: BlogComponent },
  { path: 'landing', component: LandingComponent },
  { path: '**', redirectTo: '/landing' }
];