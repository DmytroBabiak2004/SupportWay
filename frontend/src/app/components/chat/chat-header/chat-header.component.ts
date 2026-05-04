import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ProfileService } from '../../../services/profile.service';
import { RoleBadgeComponent } from '../../../shared/role-badge/role-badge.component';

@Component({
  selector: 'app-chat-header',
  standalone: true,
  imports: [CommonModule, RoleBadgeComponent],
  templateUrl: './chat-header.component.html',
  styleUrls: ['./chat-header.component.scss']
})
export class ChatHeaderComponent {
  public profileService = inject(ProfileService);
  private router = inject(Router);

  @Input() chatName: string = '';
  // Переконайся, що батьківський компонент передає сюди ID або Username
  @Input() profileKey: string | undefined | null = null;
  @Input() otherUserPhotoBase64: string | null = null;
  @Input() otherUserIsVerified: boolean | null = false;
  @Input() otherUserVerifiedAs: number | null = null;
  @Input() typingText: string | null = null;

  @Output() backClicked = new EventEmitter<void>();
  @Output() deleteClicked = new EventEmitter<void>();

  getInitials(name: string): string {
    return name ? name.charAt(0).toUpperCase() : '?';
  }

  openProfile(): void {
    this.router.navigate(['/profile', this.profileKey])
  }
}
