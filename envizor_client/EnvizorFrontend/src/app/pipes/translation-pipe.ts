import { Pipe, PipeTransform, ChangeDetectorRef } from '@angular/core';
import { TranslationService } from '../services';

@Pipe({
  name: 'translate',
  standalone: true,
  pure: false //re-run the pipe whenever a change is detected
})
export class TranslatePipe implements PipeTransform {
  private lastKey = '';
  private lastValue = '';

  constructor(
    private translationService: TranslationService,
    private cdr: ChangeDetectorRef
  ) {
    this.translationService.languageChanged$.subscribe(() => {
      this.lastValue = this.translationService.translate(this.lastKey);
      this.cdr.markForCheck(); 
    });
  }

  transform(key: string): string {
    if (key !== this.lastKey) {
      this.lastKey = key;
      this.lastValue = this.translationService.translate(key);
    }
    return this.lastValue;
  }
}