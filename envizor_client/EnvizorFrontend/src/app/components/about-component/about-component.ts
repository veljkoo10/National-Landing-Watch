import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, NgFor } from '@angular/common';
import { MatIconModule, MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';
import { TranslatePipe } from '../../pipes/translation-pipe';
import { MessageService } from '../../services';
import { MessageDto } from '../../DTOs';

@Component({
  selector: 'app-about-component',
  standalone: true,
  imports: [CommonModule, NgFor, TranslatePipe, MatIconModule, ReactiveFormsModule],
  templateUrl: './about-component.html',
  styleUrls: ['./about-component.css']
})
export class AboutComponent {
  contactForm!: FormGroup;
  isSubmitting = false;
  successMessage = '';
  errorMessage = '';

  members = [
    {
      name: 'about.team.member1.name',
      role: 'about.team.member1.role',
      phone: 'about.team.member1.phone',
      email: 'about.team.member1.email',
      img: '/images/team/viktor.jpg',
      linkedin: 'https://www.linkedin.com/in/viktor-bari%C4%87-3370122a0/',
      instagram: 'https://www.instagram.com/baricv_?igsh=MWkycGxxYjZyNjNsMQ=='
    },
    {
      name: 'about.team.member2.name',
      role: 'about.team.member2.role',
      phone: 'about.team.member2.phone',
      email: 'about.team.member2.email',
      img: '/images/team/andjela.jpg',
      linkedin: 'https://www.linkedin.com/in/an%C4%91ela-mr%C4%91a-793885280',
      instagram: 'https://www.instagram.com/_andjelamrdja_'
    },
    {
      name: 'about.team.member3.name',
      role: 'about.team.member3.role',
      phone: 'about.team.member3.phone',
      email: 'about.team.member3.email',
      img: '/images/team/milica.jpg',
      linkedin: 'https://www.linkedin.com/in/milica-mi%C5%A1an-3ab47b296/',
      instagram: 'https://www.instagram.com/misanmilica'
    },
    {
      name: 'about.team.member4.name',
      role: 'about.team.member4.role',
      phone: 'about.team.member4.phone',
      email: 'about.team.member4.email',
      img: '/images/team/veljko.jpg',
      linkedin: 'https://www.linkedin.com/in/veljkoo/',
      instagram: 'https://www.instagram.com/randjelovic__03/'
    }
  ];

  constructor(
    private iconRegistry: MatIconRegistry,
    private sanitizer: DomSanitizer,
    private fb: FormBuilder,
    private messageService: MessageService
  ) {
    this.registerIcons();
    this.initializeForm();
  }

  // Register SVG icons safely
  private registerIcons(): void {
    this.iconRegistry.addSvgIconLiteral(
      'linkedin',
      this.sanitizer.bypassSecurityTrustHtml(`
        <svg viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
          <path d="M22.225 0H1.771C.792 0 0 .771 0 1.723v20.554C0 23.229.792 24 1.771 24h20.454C23.2 24 24 23.229 24 22.277V1.723C24 .771 23.2 0 22.225 0zM7.056 20.452H3.894V9h3.162v11.452zM5.475 7.433a1.841 1.841 0 1 1 0-3.682 1.841 1.841 0 0 1 0 3.682zM20.447 20.452h-3.158v-5.568c0-1.327-.027-3.03-1.848-3.03-1.85 0-2.132 1.445-2.132 2.938v5.66H10.15V9h3.03v1.561h.043c.422-.8 1.45-1.646 2.986-1.646 3.191 0 3.238 2.742 3.238 4.92v6.617z"/>
        </svg>
      `)
    );

    this.iconRegistry.addSvgIconLiteral(
      'instagram',
      this.sanitizer.bypassSecurityTrustHtml(`
        <svg viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
          <path d="M12 2.163c3.204 0 3.584.012 4.85.07 1.366.062 2.633.35 3.608 1.325.975.975 1.262 2.242 1.324 3.608.058 1.266.069 1.646.069 4.85s-.012 3.584-.07 4.85c-.062 1.366-.35 2.633-1.324 3.608-.975.975-2.242 1.262-3.608 1.324-1.266.058-1.646.069-4.85.069s-3.584-.012-4.85-.07c-1.366-.062-2.633-.35-3.608-1.324-.975-.975-1.262-2.242-1.324-3.608C2.175 15.747 2.163 15.368 2.163 12s.012-3.584.07-4.85c.062-1.366.35-2.633 1.324-3.608C4.532 1.566 5.799 1.279 7.165 1.217 8.431 1.159 8.81 1.148 12 1.148m0-1.148C8.741 0 8.332.013 7.052.072 5.771.131 4.5.423 3.467 1.457 2.433 2.49 2.141 3.761 2.083 5.042 2.024 6.322 2.011 6.731 2.011 12s.013 5.678.072 6.958c.059 1.281.351 2.552 1.385 3.585 1.033 1.034 2.304 1.326 3.585 1.385C8.332 23.987 8.741 24 12 24s3.668-.013 4.948-.072c1.281-.059 2.552-.351 3.585-1.385 1.034-1.033 1.326-2.304 1.385-3.585.059-1.28.072-1.689.072-6.958s-.013-5.678-.072-6.958c-.059-1.281-.351-2.552-1.385-3.585C19.5.423 18.229.131 16.948.072 15.668.013 15.259 0 12 0zm0 5.838a6.162 6.162 0 1 0 .002 12.324A6.162 6.162 0 0 0 12 5.838zm0 10.162a3.999 3.999 0 1 1 .001-7.998A3.999 3.999 0 0 1 12 16.001zm6.406-11.845a1.44 1.44 0 1 0 0 2.881 1.44 1.44 0 0 0 0-2.881z"/>
        </svg>
      `)
    );
  }

  // Initialize contact form 
  private initializeForm(): void {
    this.contactForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      subject: ['', [Validators.required, Validators.minLength(3)]],
      content: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  // Send message to backend
  onSubmit(): void {
    if (this.contactForm.invalid) {
      this.errorMessage = 'Please fill in all required fields correctly.';
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';
    this.successMessage = '';

    const message: MessageDto = this.contactForm.value;

    this.messageService.sendMessage(message).subscribe({
      next: () => {
        this.successMessage = 'Your message has been sent successfully!';
        this.contactForm.reset();
      },
      error: () => {
        this.errorMessage = 'An error occurred while sending your message.';
      },
      complete: () => (this.isSubmitting = false)
    });
  }
}