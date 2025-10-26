import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../pipes/translation-pipe';

@Component({
  selector: 'app-blog',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  templateUrl: './blog-component.html',
  styleUrls: ['./blog-component.css'],
})
export class BlogComponent {
  blogs = [
    {
      id: 1,
      title: 'What is ESG and Why is it Important?',
      description:
        'Environmental, Social, and Governance (ESG) isn’t just an investor’s checklist anymore — it’s now a...',
      image:
        '/images/esg.webp',
      link: 'https://esgthereport.com/what-is-esg-and-why-is-it-important/',
    },
    {
      id: 2,
      title: 'How Generative AI can build an organization’s ESG roadmap',
      description:
        'ESG reporting has emerged as a critical component of corporate strategy and stakeholder engagement...',
      image:
        '/images/AI.jpeg',
      link: 'https://www.ey.com/en_in/insights/ai/how-generative-ai-can-build-an-organization-s-esg-roadmap',
    },
    {
      id: 3,
      title: 'How Does Climate Change Affect Investing?',
      description:
        'Climate change is not just an environmental issue—it’s a financial one too. As global temperatures...',
      image:
        '/images/biznis.webp',
      link: 'https://esgthereport.com/how-does-climate-change-affect-investing/',
    },
    {
      id: 4,
      title: 'How Can the ESG Disclosure Framework Help your Business?',
      description:
        'In today’s business landscape, transparency and sustainability are more crucial than ever. Imagine...',
      image:
        '/images/hand.webp',
      link: 'https://esgthereport.com/how-can-the-esg-disclosure-framework-help-your-business/',
    },
    {
      id: 5,
      title: 'ESG Survey: Who Should be Fined for Greenwashing?',
      description:
        'Welcome to our deep dive into a rising issue that impacts consumers worldwide.',
      image:
        '/images/tree.webp',
      link: 'https://esgthereport.com/esg-survey-advisers-say-asset-managers-should-be-fined-for-greenwashing/',
    },
    {
      id: 6,
      title: 'What is Social and Environmental Reporting?',
      description:
        'Social and environmental reporting plays a crucial role in helping businesses...',
      image:
        '/images/esgg.webp',
      link: 'https://esgthereport.com/what-is-social-and-environmental-reporting/',
    },
    {
      id: 7,
      title: 'What are ESG Certifications?',
      description:
        'A certificate in ESG investing is a rigorous credential that signifies your environmental...',
      image:
        '/images/sert.webp',
      link: 'https://esgthereport.com/what-are-esg-certifications/',
    },
    {
      id: 8,
      title: 'What does Personal ESG have to do with Green Living?',
      description:
        'Green living has become more than just a trend – it’s a lifestyle choice for many ...',
      image:
        '/images/house.webp',
      link: 'https://esgthereport.com/what-does-personal-esg-have-to-do-with-green-living/',
    },
  ];
}
