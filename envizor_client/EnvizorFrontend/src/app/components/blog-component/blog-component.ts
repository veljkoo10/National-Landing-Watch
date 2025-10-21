import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-blog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './blog-component.html',
  styleUrls: ['./blog-component.css'],
})
export class BlogComponent {
  blogs = [
    {
      id: 1,
      title: 'The Role of ESG in a Sustainable Future',
      description:
        'Environmental, Social, and Governance (ESG) principles are now central to responsible business. Learn how ESG metrics influence investments and help build a resilient global economy.',
      image:
        'https://images.unsplash.com/photo-1518837695005-2083093ee35b?auto=format&fit=crop&w=1200&q=80',
      link: 'https://www.un.org/sustainabledevelopment/sustainable-development-goals/',
    },
    {
      id: 2,
      title: 'Methane Emissions and Climate Change',
      description:
        'Methane is one of the most potent greenhouse gases. Explore its sources, effects, and the technologies that help detect and reduce emissions worldwide.',
      image:
        'https://images.unsplash.com/photo-1518837695005-2083093ee35b?auto=format&fit=crop&w=1200&q=80',
      link: 'https://www.epa.gov/gmi/methane-emissions',
    },
    {
      id: 3,
      title: 'Carbon Dioxide: The Invisible Challenge',
      description:
        'CO₂ is a natural part of our atmosphere, but human activities have disrupted the balance. Find out how innovative solutions are being developed to capture and store carbon.',
      image:
        'https://images.unsplash.com/photo-1509395176047-4a66953fd231?auto=format&fit=crop&w=1200&q=80',
      link: 'https://www.climatewatchdata.org/ghg-emissions',
    },
    {
      id: 4,
      title: 'Circular Economy: Rethinking Waste',
      description:
        'From plastic recycling to sustainable production, the circular economy model aims to eliminate waste and promote resource efficiency.',
      image:
        'https://images.unsplash.com/photo-1518837695005-2083093ee35b?auto=format&fit=crop&w=1200&q=80',
      link: 'https://ellenmacarthurfoundation.org/topics/circular-economy-introduction/overview',
    },
    {
      id: 5,
      title: 'Green Energy and the Path to Net Zero',
      description:
        'Renewable energy sources like wind, solar, and hydro are essential in achieving net-zero emissions. Learn how nations are transitioning to greener energy systems.',
      image:
        'https://images.unsplash.com/photo-1509395176047-4a66953fd231?auto=format&fit=crop&w=1200&q=80',
      link: 'https://www.iea.org/topics/renewables',
    },
    {
      id: 6,
      title: 'Green Energy and the Path to Net Zero',
      description:
        'Renewable energy sources like wind, solar, and hydro are essential in achieving net-zero emissions. Learn how nations are transitioning to greener energy systems.',
      image:
        'https://images.unsplash.com/photo-1509395176047-4a66953fd231?auto=format&fit=crop&w=1200&q=80',
      link: 'https://www.iea.org/topics/renewables',
    },
  ];
}
