import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ChatService } from '../../../services/chat.service';
import { ChatListItem } from '../../../models/chat.model';

@Component({
  selector: 'app-share-to-chat-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './share-to-chat-modal.component.html',
  styleUrls: ['./share-to-chat-modal.component.scss']
})
export class ShareToChatModalComponent implements OnInit {
  private chatService = inject(ChatService);

  @Input({ required: true }) entityType!: 'post' | 'helpRequest';
  @Input({ required: true }) entityId!: string;
  @Input() previewTitle: string | null = null;
  @Input() previewContent: string | null = null;

  @Output() closed = new EventEmitter<void>();
  @Output() shared = new EventEmitter<void>();

  chats: ChatListItem[] = [];
  filteredChats: ChatListItem[] = [];

  selectedChatId: string | null = null;
  search = '';
  caption = '';
  isLoading = false;
  isSubmitting = false;
  error = '';

  ngOnInit(): void {
    this.loadChats();
  }

  loadChats(): void {
    this.isLoading = true;
    this.chatService.getChats().subscribe({
      next: chats => {
        this.chats = chats ?? [];
        this.filteredChats = [...this.chats];
        this.isLoading = false;
      },
      error: () => {
        this.error = 'Не вдалося завантажити чати';
        this.isLoading = false;
      }
    });
  }

  filterChats(): void {
    const q = this.search.trim().toLowerCase();

    if (!q) {
      this.filteredChats = [...this.chats];
      return;
    }

    this.filteredChats = this.chats.filter(chat =>
      (chat.displayName ?? '').toLowerCase().includes(q) ||
      (chat.userName ?? '').toLowerCase().includes(q)
    );
  }

  selectChat(chatId: string): void {
    this.selectedChatId = chatId;
  }

  async submit(): Promise<void> {
    if (!this.selectedChatId || this.isSubmitting) return;

    this.isSubmitting = true;
    this.error = '';

    try {
      if (this.entityType === 'post') {
        await this.chatService.sharePost(this.selectedChatId, this.entityId, this.caption);
      } else {
        await this.chatService.shareHelpRequest(this.selectedChatId, this.entityId, this.caption);
      }

      this.shared.emit();
      this.close();
    } catch (e) {
      console.error(e);
      this.error = 'Не вдалося поділитися в чат';
    } finally {
      this.isSubmitting = false;
    }
  }

  close(): void {
    this.closed.emit();
  }

  getShortContent(text?: string | null): string {
    const value = text?.trim() ?? '';
    if (!value) return '';
    return value.length > 140 ? `${value.slice(0, 140)}…` : value;
  }

  // Метод для генерації ініціалів, якщо немає аватарки
  getInitials(name?: string): string {
    if (!name) return '?';
    return name.substring(0, 2).toUpperCase();
  }

  // Обгортка для типізації (якщо у вашій моделі ChatListItem є avatarUrl)
  getAvatar(chat: any): string | null {
    return chat.avatarUrl || chat.avatar || null;
  }

  trackByChatId(_: number, chat: ChatListItem): string {
    return chat.id;
  }
}
