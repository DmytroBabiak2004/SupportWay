import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { HelpRequest } from '../../models/help-request.model';

@Component({
  selector: 'app-help-request-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './help-request.component.html',
  styleUrls: ['./help-request.component.scss']
})
export class HelpRequestCardComponent {
  private router = inject(Router);
  @Input({ required: true }) request!: HelpRequest;

  openProfile() {
    this.router.navigate(['/profile', this.request.userId]);
  }

  getInitials(name: string): string {
    return name ? name[0].toUpperCase() : '?';
  }

  getStatusClass(status: string): string {
    // Повертає клас CSS залежно від назви статусу
    const s = status?.toLowerCase() || '';
    if (s.includes('актив') || s.includes('active')) return 'badge-active';
    if (s.includes('закрит') || s.includes('closed')) return 'badge-closed';
    return 'badge-default';
  }
}
