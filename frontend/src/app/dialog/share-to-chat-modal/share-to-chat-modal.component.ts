import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ChatService } from '../../services/chat.service';
import { ChatListItem } from '../../models/chat.model';

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
    this.error = '';

    this.chatService.getChats().subscribe({
      next: chats => {
        this.chats = chats ?? [];
        this.filteredChats = [...this.chats];
        this.isLoading = false;
      },
      error: error => {
        console.error('Share modal chats loading error:', error);
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

    this.filteredChats = this.chats.filter(chat => {
      const displayName = this.getChatDisplayName(chat).toLowerCase();
      const userName = this.getStringField(chat, [
        'userName',
        'username',
        'interlocutorUserName',
        'interlocutorUsername',
        'receiverUserName',
        'receiverUsername',
        'participantUserName',
        'participantUsername'
      ]).toLowerCase();

      const lastMessage = this.getLastMessage(chat).toLowerCase();

      return (
        displayName.includes(q) ||
        userName.includes(q) ||
        lastMessage.includes(q)
      );
    });
  }

  selectChat(chatId: string): void {
    this.selectedChatId = chatId;
  }

  async submit(): Promise<void> {
    if (!this.selectedChatId || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;
    this.error = '';

    try {
      const cleanCaption = this.caption.trim();

      if (this.entityType === 'post') {
        await this.chatService.sharePost(this.selectedChatId, this.entityId, cleanCaption);
      } else {
        await this.chatService.shareHelpRequest(this.selectedChatId, this.entityId, cleanCaption);
      }

      this.shared.emit();
      this.close();
    } catch (error) {
      console.error('Share to chat error:', error);
      this.error = 'Не вдалося поділитися в чат';
    } finally {
      this.isSubmitting = false;
    }
  }

  close(): void {
    this.closed.emit();
  }

  getEntityLabel(): string {
    return this.entityType === 'post'
      ? 'Публікація'
      : 'Запит допомоги';
  }

  getShortContent(text?: string | null): string {
    const value = text?.trim() ?? '';
    if (!value) return '';

    return value.length > 130
      ? `${value.slice(0, 130)}…`
      : value;
  }

  getChatDisplayName(chat: ChatListItem): string {
    return this.getStringField(chat, [
      'displayName',
      'name',
      'title',
      'userName',
      'username',
      'interlocutorName',
      'interlocutorFullName',
      'interlocutorUserName',
      'interlocutorUsername',
      'receiverName',
      'receiverFullName',
      'receiverUserName',
      'receiverUsername',
      'participantName',
      'participantFullName',
      'participantUserName',
      'participantUsername'
    ]) || 'Чат';
  }

  getLastMessage(chat: ChatListItem): string {
    return this.getStringField(chat, [
      'lastMessage',
      'lastMessageText',
      'lastMessageContent',
      'lastMessagePreview',
      'message',
      'content'
    ]);
  }

  getInitials(name?: string | null): string {
    if (!name?.trim()) {
      return '?';
    }

    const parts = name
      .trim()
      .split(/\s+/)
      .slice(0, 2);

    return parts
      .map(part => part.charAt(0).toUpperCase())
      .join('') || '?';
  }

  getAvatar(chat: ChatListItem): string | null {
    const raw = this.getStringField(chat, [
      'avatarUrl',
      'avatar',
      'photoUrl',
      'photo',
      'photoBase64',
      'profilePhotoBase64',
      'userPhotoBase64',
      'authorPhotoBase64',
      'interlocutorPhotoBase64',
      'interlocutorAvatarBase64',
      'interlocutorAvatar',
      'interlocutorPhoto',
      'receiverPhotoBase64',
      'receiverAvatarBase64',
      'receiverAvatar',
      'receiverPhoto',
      'participantPhotoBase64',
      'participantAvatarBase64',
      'participantAvatar',
      'participantPhoto'
    ]);

    return this.toImageSrc(raw);
  }

  trackByChatId(_: number, chat: ChatListItem): string {
    return chat.id;
  }

  private getStringField(source: unknown, keys: string[]): string {
    if (!source || typeof source !== 'object') {
      return '';
    }

    const direct = source as Record<string, unknown>;

    for (const key of keys) {
      const value = direct[key];

      if (typeof value === 'string' && value.trim()) {
        return value.trim();
      }
    }

    const nestedSources = [
      direct['user'],
      direct['profile'],
      direct['interlocutor'],
      direct['receiver'],
      direct['participant'],
      direct['otherUser'],
      direct['member']
    ];

    for (const nested of nestedSources) {
      if (!nested || typeof nested !== 'object') {
        continue;
      }

      const nestedRecord = nested as Record<string, unknown>;

      for (const key of keys) {
        const value = nestedRecord[key];

        if (typeof value === 'string' && value.trim()) {
          return value.trim();
        }
      }
    }

    return '';
  }

  private toImageSrc(raw?: string | null): string | null {
    if (!raw || typeof raw !== 'string') {
      return null;
    }

    const value = raw.trim();

    if (!value) {
      return null;
    }

    if (
      value.startsWith('data:image/') ||
      value.startsWith('http://') ||
      value.startsWith('https://') ||
      value.startsWith('blob:')
    ) {
      return value;
    }

    return `data:image/jpeg;base64,${value}`;
  }
}
