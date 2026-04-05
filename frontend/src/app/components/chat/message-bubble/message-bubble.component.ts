import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

import { Message, ChatService } from '../../../services/chat.service';
import { ProfileService } from '../../../services/profile.service';

@Component({
  selector: 'app-message-bubble',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './message-bubble.component.html',
  styleUrls: ['./message-bubble.component.scss']
})
export class MessageBubbleComponent {
  private router = inject(Router);
  public chatService = inject(ChatService);
  public profileService = inject(ProfileService);

  @Input() msg!: Message;
  @Input() isMine = false;

  @Output() deleteRequest = new EventEmitter<string>();

  onDelete(): void {
    this.deleteRequest.emit(this.msg.id);
  }

  isShared(): boolean {
    return !!this.msg.sharedPreview;
  }

  openSharedEntity(): void {
    const preview = this.msg.sharedPreview;
    if (!preview) return;

    if (preview.entityType === 'helpRequest') {
      this.router.navigate(['/requests']);
    } else {
      this.router.navigate(['/posts']);
    }
  }

  getPreviewImage(): string | null {
    return this.chatService.getPreviewImageSrc(this.msg.sharedPreview?.imageBase64);
  }

  getPreviewText(text?: string | null): string {
    const value = text?.trim() ?? '';
    if (!value) return '';
    return value.length > 180 ? `${value.slice(0, 180)}…` : value;
  }

  getEntityLabel(): string {
    const type = this.msg.sharedPreview?.entityType;
    if (type === 'helpRequest') return 'Запит допомоги';
    return 'Пост';
  }
}
