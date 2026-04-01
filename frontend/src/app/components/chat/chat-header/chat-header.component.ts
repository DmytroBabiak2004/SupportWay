import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ProfileService } from '../../../services/profile.service';

@Component({
  selector: 'app-chat-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chat-header.component.html',
  styleUrls: ['./chat-header.component.scss']
})
export class ChatHeaderComponent {
  public profileService = inject(ProfileService);
  private router = inject(Router);

  @Input() chatName: string = '';
  @Input() profileKey: string | null = null;
  @Input() otherUserPhotoBase64: string | null = null;
  @Input() typingText: string | null = null;

  @Output() backClicked = new EventEmitter<void>();
  @Output() deleteClicked = new EventEmitter<void>();

  getInitials(name: string): string {
    return name ? name[0].toUpperCase() : '?';
  }

  openProfile(): void {
    if (!this.profileKey) return;
    this.router.navigate(['/profile', this.profileKey]);
  }
}
