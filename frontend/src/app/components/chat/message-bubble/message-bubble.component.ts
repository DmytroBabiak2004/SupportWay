import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Message, ChatService } from '../../../services/chat.service';
import { ProfileService } from '../../../services/profile.service';
import { SharedPostDialogComponent } from '../../../dialog/shared-post-modal/shared-post-dialog.component';

@Component({
  selector: 'app-message-bubble',
  standalone: true,
  imports: [CommonModule, SharedPostDialogComponent],
  templateUrl: './message-bubble.component.html',
  styleUrls: ['./message-bubble.component.scss']
})
export class MessageBubbleComponent {
  public chatService = inject(ChatService);
  public profileService = inject(ProfileService);

  @Input() msg!: Message;
  @Input() isMine = false;

  @Output() deleteRequest = new EventEmitter<string>();

  isPostModalOpen = false;

  onDelete(event?: MouseEvent): void {
    event?.stopPropagation();
    this.deleteRequest.emit(this.msg.id);
  }

  isShared(): boolean {
    return !!this.msg.sharedPreview;
  }

  openSharedEntity(event?: MouseEvent): void {
    event?.stopPropagation();

    if (!this.msg.sharedPreview) {
      return;
    }

    this.isPostModalOpen = true;
  }

  closePostModal(): void {
    this.isPostModalOpen = false;
  }

  getPreviewImage(): string | null {
    return this.chatService.getPreviewImageSrc(this.msg.sharedPreview?.imageBase64);
  }

  getPreviewText(text?: string | null, max = 150): string {
    const value = text?.trim() ?? '';
    if (!value) return '';
    return value.length > max ? `${value.slice(0, max)}…` : value;
  }

  getEntityLabel(): string {
    return this.msg.sharedPreview?.entityType === 'helpRequest'
      ? 'Запит допомоги'
      : 'Пост';
  }

  formatTime(value?: string | Date | null): string {
    if (!value) return '';

    return new Intl.DateTimeFormat('uk-UA', {
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(value));
  }

  getReadLabel(): string {
    return this.msg.isRead ? 'Прочитано' : 'Надіслано';
  }
}
