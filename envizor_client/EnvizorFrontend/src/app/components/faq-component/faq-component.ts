import { Component } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { NgFor } from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';

@Component({
  selector: 'app-faq-component',
  standalone: true,
  imports: [MatExpansionModule, MatIconModule, NgFor, TranslatePipe],
  templateUrl: './faq-component.html',
  styleUrls: ['./faq-component.css']
})
export class FaqComponent {
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

  scrollToCategory(id: string): void {
    const element = document.getElementById(id);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}