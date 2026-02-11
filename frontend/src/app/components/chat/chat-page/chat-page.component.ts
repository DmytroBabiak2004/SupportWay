import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

// ✅ Додав MessageDeletedEvent в імпорти
import {
  ChatService,
  Chat,
  Message,
  ReadReceiptEvent,
  MessageDeletedEvent
} from '../../../services/chat.service';
import { AuthService } from '../../../services/auth.service';

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
    EmptyStateComponent,
  ],
  templateUrl: './chat-page.component.html',
  styleUrls: ['./chat-page.component.scss'],
})
export class ChatPageComponent implements OnInit, OnDestroy {
  chats: Chat[] = [];
  selectedChat: Chat | null = null;

  messages: Message[] = [];
  newMessage = '';

  userId!: string;
  typingText: string | null = null;

  // mobile drawer state
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
    } catch (err) {
      console.error('SignalR Error:', err);
    }

    // 1. ОТРИМАННЯ ПОВІДОМЛЕНЬ
    this.subs.add(
      this.chatService.messageReceived$.subscribe((msg: Message) => {
        if (this.selectedChat && msg.chatId === this.selectedChat.id) {
          this.messages = [...this.messages, msg];

          if (msg.senderId !== this.userId) {
            this.chatService.markAsRead(this.selectedChat.id, msg.id);
          }
        }
      })
    );

    // 2. ОТРИМАННЯ ЗВІТУ ПРО ПРОЧИТАННЯ
    this.subs.add(
      this.chatService.readReceipt$.subscribe((event: ReadReceiptEvent) => {
        if (
          this.selectedChat &&
          event.chatId === this.selectedChat.id &&
          event.userId !== this.userId
        ) {
          this.updateLocalReadStatus(event.lastReadMessageId);
        }
      })
    );

    // 3. ІНДИКАТОР НАБОРУ
    this.subs.add(
      this.chatService.typingEvent$.subscribe(event => {
        if (
          this.selectedChat &&
          event.chatId === this.selectedChat.id &&
          event.userId !== this.userId
        ) {
          this.showTypingIndicator();
        }
      })
    );

    // ✅ 4. ОБРОБКА ВИДАЛЕННЯ (REAL-TIME)
    // Слухаємо, коли сервер каже, що повідомлення видалено
    this.subs.add(
      this.chatService.messageDeleted$.subscribe((event: MessageDeletedEvent) => {
        // Перевіряємо, чи ми зараз у тому чаті, де сталося видалення
        if (this.selectedChat && event.chatId === this.selectedChat.id) {
          // Фільтруємо список: залишаємо всі повідомлення, крім видаленого
          this.messages = this.messages.filter(m => m.id !== event.messageId);
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.chatService.stopConnection();
    if (this.typingTimer) clearTimeout(this.typingTimer);
  }

  loadChats(): void {
    this.subs.add(
      this.chatService.getChats().subscribe({
        next: (chats) => {
          this.chats = chats;
          this.checkUrlForChat();
        },
        error: (err) => console.error(err),
      })
    );
  }

  private checkUrlForChat(): void {
    this.subs.add(
      this.route.queryParams.subscribe(params => {
        const chatId = params['chatId'];
        if (!chatId) return;

        const chat = this.chats.find(c => c.id === chatId);
        if (chat) this.openChat(chat);
      })
    );
  }

  openChat(chat: Chat): void {
    this.selectedChat = chat;
    this.messages = [];
    this.isSidebarOpen = false;

    this.subs.add(
      this.chatService.getMessages(chat.id).subscribe({
        next: (msgs) => {
          this.messages = msgs;
          this.sendReadReceiptForLastMessage();
        },
        error: (err) => console.error(err),
      })
    );
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

  async sendMessage(): Promise<void> {
    if (!this.newMessage.trim() || !this.selectedChat) return;

    try {
      await this.chatService.sendMessage(this.selectedChat.id, this.newMessage);
      this.newMessage = '';
    } catch (err) {
      console.error(err);
    }
  }

  onTyping(): void {
    if (this.selectedChat) this.chatService.sendTyping(this.selectedChat.id);
  }

  private showTypingIndicator(): void {
    this.typingText = 'друкує...';
    if (this.typingTimer) clearTimeout(this.typingTimer);

    this.typingTimer = setTimeout(() => {
      this.typingText = null;
    }, 3000);
  }

  getChatName(chat: Chat): string {
    if ((chat as any).name) return (chat as any).name;
    const other = chat.userChats?.find(uc => uc.userId !== this.userId);
    return other?.user.userName ?? 'Користувач';
  }

  private sendReadReceiptForLastMessage(): void {
    if (!this.selectedChat || this.messages.length === 0) return;

    const lastMsg = this.messages[this.messages.length - 1];

    if (lastMsg.senderId !== this.userId) {
      this.chatService.markAsRead(this.selectedChat.id, lastMsg.id);
    }
  }

  private updateLocalReadStatus(lastReadId: string): void {
    const readIndex = this.messages.findIndex(m => m.id === lastReadId);

    if (readIndex !== -1) {
      this.messages = this.messages.map((msg, index) => {
        if (index <= readIndex && msg.senderId === this.userId) {
          return { ...msg, isRead: true };
        }
        return msg;
      });
    }
  }

  onDeleteChatFromHeader() {
    const chat = this.selectedChat;
    if (!chat) return;

    const chatName = this.getChatName(chat);

    if (confirm(`Ви точно хочете видалити чат "${chatName}"?`)) {
      this.chatService.deleteChat(chat.id).subscribe({
        next: () => {
          this.chats = this.chats.filter(c => c.id !== chat.id);
          this.closeChat();
        },
        error: (err) => {
          console.error('Помилка видалення:', err);
          alert('Не вдалося видалити чат');
        }
      });
    }
  }

  // ✅ НОВИЙ МЕТОД: Обробка видалення окремого повідомлення
  async onDeleteMessage(messageId: string): Promise<void> {
    if (!confirm('Видалити це повідомлення?')) return;

    try {
      // 1. Відправляємо запит на сервер
      await this.chatService.deleteMessage(messageId);

      // 2. Локально нічого не видаляємо вручну.
      // Ми чекаємо на подію messageDeleted$ (у ngOnInit),
      // яка прийде через SignalR і оновить масив this.messages.
    } catch (err) {
      console.error('Error deleting message:', err);
      alert('Не вдалося видалити повідомлення');
    }
  }
}
