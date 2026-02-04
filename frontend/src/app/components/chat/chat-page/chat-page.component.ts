import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import { ChatService, Chat, Message } from '../../../services/chat.service';
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

  // ✅ mobile drawer state
  isSidebarOpen = true;

  private subs = new Subscription();
  private typingTimer?: ReturnType<typeof setTimeout>;

  constructor(
    private chatService: ChatService,
    private authService: AuthService,
    private route: ActivatedRoute
  ) {}

  async ngOnInit(): Promise<void> {
    this.userId = this.authService.getUserId();
    this.loadChats();

    try {
      await this.chatService.startSignalR();
    } catch (err) {
      console.error('SignalR Error:', err);
    }

    this.subs.add(
      this.chatService.messageReceived$.subscribe((msg: Message) => {
        if (this.selectedChat && msg.chatId === this.selectedChat.id) {
          // message-list сам скролить вниз
          this.messages = [...this.messages, msg];
        }
      })
    );

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

    // ✅ на телефоні ховаємо список після вибору
    this.isSidebarOpen = false;

    this.subs.add(
      this.chatService.getMessages(chat.id).subscribe({
        next: (msgs) => {
          this.messages = msgs; // message-list сам автоскролить
        },
        error: (err) => console.error(err),
      })
    );
  }

  closeChat(): void {
    this.selectedChat = null;
    this.messages = [];

    // ✅ на телефоні повертаємо список
    this.isSidebarOpen = true;
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

  onDeleteChatFromHeader() {
    // 1. Створюємо локальну константу. Це фіксує значення.
    const chat = this.selectedChat;

    // 2. Перевіряємо локальну змінну
    if (!chat) return;

    // Тепер TypeScript точно знає, що 'chat' не є null у всьому цьому блоці
    const confirmDelete = confirm(`Ви точно хочете видалити чат "${chat.name}"?`);

    if (confirmDelete) {
      // 3. Використовуємо 'chat.id' замість 'this.selectedChat.id'
      this.chatService.deleteChat(chat.id).subscribe(() => {
        this.chats = this.chats.filter(c => c.id !== chat.id);
        this.selectedChat = null;
        // Якщо є масив повідомлень, очищаємо його
        // this.messages = [];
      });
    }
  }
}
