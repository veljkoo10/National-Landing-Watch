import { Routes } from '@angular/router';
import { MapComponent } from '../app/components/map-component/map-component';

export const routes: Routes = [
  { path: '', redirectTo: '/map', pathMatch: 'full' },
  { path: 'map', component: MapComponent },
  { path: 'faq', loadComponent: () => import('../app/components/faq-component/faq-component').then(m => m.FaqComponent) },
  { path: 'about', loadComponent: () => import('../app/components/about-component/about-component').then(m => m.AboutComponent) },
  { path: 'statistics', loadComponent: () => import('../app/components/statistics-component/statistics-component').then(m => m.StatisticsComponent) },
  { path: 'blog', loadComponent: () => import('../app/components/blog-component/blog-component').then(m => m.BlogComponent) },
  { path: '**', redirectTo: '/map' }
];
