import { Component } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import {NgClass, NgFor} from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';
import {Router} from '@angular/router';

@Component({
  selector: 'app-faq-component',
  standalone: true,
  imports: [MatExpansionModule, MatIconModule, NgFor, NgClass, TranslatePipe],
  templateUrl: './faq-component.html',
  styleUrls: ['./faq-component.css']
})
export class FaqComponent {
  constructor(private router: Router) {
  }

  categories = [
    {
      id: 'map',
      title: 'faq.categories.map.title',
      icon: 'public',
      questions: [
        {
          question: 'faq.categories.map.q1.question',
          answer: 'faq.categories.map.q1.answer'
        },
        {
          question: 'faq.categories.map.q2.question',
          answer: 'faq.categories.map.q2.answer'
        }
      ]
    },
    {
      id: 'statistics',
      title: 'faq.categories.statistics.title',
      icon: 'bar_chart',
      questions: [
        {
          question: 'faq.categories.statistics.q1.question',
          answer: 'faq.categories.statistics.q1.answer'
        },
        {
          question: 'faq.categories.statistics.q2.question',
          answer: 'faq.categories.statistics.q2.answer'
        }
      ]
    },
    {
      id: 'info',
      title: 'faq.categories.info.title',
      icon: 'info_outline',
      questions: [
        {
          question: 'faq.categories.info.q1.question',
          answer: 'faq.categories.info.q1.answer'
        },
        {
          question: 'faq.categories.info.q2.question',
          answer: 'faq.categories.info.q2.answer'
        }
      ]
    }
  ];

  navigateTo(id: string): void {
    switch (id) {
      case 'map':
        this.router.navigate(['/map']);
        break;
      case 'statistics':
        this.router.navigate(['/statistics']);
        break;
      case 'info':
        this.router.navigate(['/about']);
        break;
      default:
        const element = document.getElementById(id);
        if (element) element.scrollIntoView({behavior: 'smooth', block: 'start'});
        break;
    }
  }
}
