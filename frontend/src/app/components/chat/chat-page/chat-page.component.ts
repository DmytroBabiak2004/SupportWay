import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { ChatService, Message, ReadReceiptEvent, MessageDeletedEvent } from '../../../services/chat.service';
import { AuthService } from '../../../services/auth.service';
import { ChatListItem } from '../../../models/chat.model';

import { ChatSidebarComponent } from '../chat-sidebar/chat-sidebar.component';
import { ChatHeaderComponent } from '../chat-header/chat-header.component';
import { MessageListComponent } from '../message-list/message-list.component';
import { ChatComposerComponent } from '../chat-composer/chat-composer.component';
import { EmptyStateComponent } from '../empty-state/empty-state.component';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ChatSidebarComponent,
    ChatHeaderComponent,
    MessageListComponent,
    ChatComposerComponent,
    EmptyStateComponent
  ],
  templateUrl: './chat-page.component.html',
  styleUrls: ['./chat-page.component.scss'],
})
export class ChatPageComponent implements OnInit, OnDestroy {
  chats: ChatListItem[] = [];
  selectedChat: ChatListItem | null = null;
  messages: Message[] = [];
  userId!: string;
  typingText: string | null = null;
  isSidebarOpen = true;

  private subs = new Subscription();
  private typingTimer?: ReturnType<typeof setTimeout>;

  constructor(
    private chatService: ChatService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.userId = this.authService.getUserId();
    this.loadChats();

    try {
      await this.chatService.startSignalR();
    } catch (e) {
      console.error(e);
    }

    this.subs.add(this.chatService.messageReceived$.subscribe((msg: Message) => {
      if (this.selectedChat && msg.chatId === this.selectedChat.id) {
        this.messages = [...this.messages, msg];

        if (msg.senderId !== this.userId) {
          this.chatService.markAsRead(this.selectedChat.id, msg.id);
        }
      }

      const sidebarPreview =
        msg.sharedPreview?.title ||
        msg.sharedPreview?.content ||
        msg.content ||
        'Нове повідомлення';

      this.chats = this.chats.map(c =>
        c.id === msg.chatId
          ? {
            ...c,
            lastMessage: sidebarPreview,
            lastMessageAt: msg.sentAt,
            unreadCount: c.id === this.selectedChat?.id ? 0 : c.unreadCount + 1
          }
          : c
      );
    }));

    this.subs.add(this.chatService.readReceipt$.subscribe((e: ReadReceiptEvent) => {
      if (this.selectedChat?.id === e.chatId && e.userId !== this.userId) {
        this.updateLocalReadStatus(e.lastReadMessageId);
      }
    }));

    this.subs.add(this.chatService.typingEvent$.subscribe(e => {
      if (this.selectedChat?.id === e.chatId && e.userId !== this.userId) {
        this.showTypingIndicator();
      }
    }));

    this.subs.add(this.chatService.messageDeleted$.subscribe((e: MessageDeletedEvent) => {
      if (this.selectedChat?.id === e.chatId) {
        this.messages = this.messages.filter(m => m.id !== e.messageId);
      }
    }));
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.chatService.stopConnection();

    if (this.typingTimer) {
      clearTimeout(this.typingTimer);
    }
  }

  loadChats(): void {
    this.subs.add(this.chatService.getChats().subscribe({
      next: chats => {
        this.chats = chats;
        this.checkUrlForChat();
      },
      error: console.error
    }));
  }

  private checkUrlForChat(): void {
    this.subs.add(this.route.queryParams.subscribe(params => {
      const chatId = params['chatId'];
      if (!chatId) return;

      const chat = this.chats.find(c => c.id === chatId);
      if (chat) {
        this.openChat(chat);
      }
    }));
  }

  openChat(chat: ChatListItem): void {
    this.selectedChat = chat;
    this.messages = [];
    this.isSidebarOpen = false;

    this.chats = this.chats.map(c =>
      c.id === chat.id ? { ...c, unreadCount: 0 } : c
    );

    this.subs.add(this.chatService.getMessages(chat.id).subscribe({
      next: msgs => {
        this.messages = msgs;
        this.sendReadReceiptForLast();
      },
      error: console.error
    }));
  }

  closeChat(): void {
    this.selectedChat = null;
    this.messages = [];
    this.isSidebarOpen = true;

    this.router.navigate([], {
      queryParams: { chatId: null },
      queryParamsHandling: 'merge'
    });
  }

  async sendMessage(text: string): Promise<void> {
    if (!text.trim() || !this.selectedChat) return;

    try {
      await this.chatService.sendMessage(this.selectedChat.id, text);
    } catch (e) {
      console.error(e);
    }
  }

  onTyping(): void {
    if (this.selectedChat) {
      this.chatService.sendTyping(this.selectedChat.id);
    }
  }

  async onDeleteMessage(messageId: string): Promise<void> {
    if (!confirm('Видалити це повідомлення?')) return;

    try {
      await this.chatService.deleteMessage(messageId);
    } catch {
      alert('Не вдалося видалити повідомлення');
    }
  }

  onDeleteChat(): void {
    if (!this.selectedChat) return;

    if (!confirm(`Видалити чат з "${this.selectedChat.displayName}"?`)) return;

    this.chatService.deleteChat(this.selectedChat.id).subscribe({
      next: () => {
        this.chats = this.chats.filter(c => c.id !== this.selectedChat!.id);
        this.closeChat();
      }
    });
  }

  private showTypingIndicator(): void {
    this.typingText = 'друкує...';

    if (this.typingTimer) {
      clearTimeout(this.typingTimer);
    }

    this.typingTimer = setTimeout(() => {
      this.typingText = null;
    }, 3000);
  }

  private sendReadReceiptForLast(): void {
    if (!this.selectedChat || !this.messages.length) return;

    const last = this.messages[this.messages.length - 1];
    if (last.senderId !== this.userId) {
      this.chatService.markAsRead(this.selectedChat.id, last.id);
    }
  }

  private updateLocalReadStatus(lastReadId: string): void {
    const idx = this.messages.findIndex(m => m.id === lastReadId);
    if (idx === -1) return;

    this.messages = this.messages.map((m, i) =>
      i <= idx && m.senderId === this.userId
        ? { ...m, isRead: true }
        : m
    );
  }

  onSidebarChatDeleted(deletedChatId: string | number): void {
    this.chats = this.chats.filter(c => c.id !== deletedChatId);
  }
}
