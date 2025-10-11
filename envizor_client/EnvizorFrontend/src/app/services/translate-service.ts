import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private currentLang: string = 'en';
  private translations: any = {};
  private languageSubject = new BehaviorSubject<string>(this.currentLang);
  languageChanged$ = this.languageSubject.asObservable();

  constructor(private http: HttpClient) {
    const savedLang = localStorage.getItem('lang');
    this.currentLang = savedLang || 'en';

    this.loadTranslations(this.currentLang);
    }

  setLanguage(lang: string) {
    if (lang === this.currentLang) return;
    this.currentLang = lang;
    localStorage.setItem('lang', lang);

    this.loadTranslations(lang);
    }

  getLanguage(): string {
    return this.currentLang;
  }

  private loadTranslations(lang: string) {
    this.http.get(`/i18n/${lang}.json`).subscribe({
      next: (data) => {
        this.translations = data;
        this.languageSubject.next(lang); 
      },
      error: (err) => console.error(`Failed to load ${lang} translations`, err)
    });
  }

  translate(key: string): string {
    return this.translations[key] || key;
  }
}