import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';

import { ChatService, Chat, Message } from '../../services/chat.service';
import { AuthService } from '../../services/auth.service';
import { ChatSidebarComponent } from '../chat/chat-sidebar/chat-sidebar.component';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ChatSidebarComponent],
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

  isSidebarOpen = false;

  @ViewChild('scrollBox') private scrollBox?: ElementRef<HTMLElement>;

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
          this.messages = [...this.messages, msg];
          setTimeout(() => this.autoScroll(), 50);
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

    this.isSidebarOpen = !this.selectedChat;
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
          setTimeout(() => this.autoScroll(), 100);
        },
        error: (err) => console.error(err),
      })
    );
  }

  closeChat(): void {
    this.selectedChat = null;
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

  private autoScroll(): void {
    const el = this.scrollBox?.nativeElement;
    if (!el) return;
    el.scrollTop = el.scrollHeight;
  }

  getChatName(chat: Chat): string {
    if (chat.name) return chat.name;
    const other = chat.userChats?.find(uc => uc.userId !== this.userId);
    return other?.user.userName ?? 'Користувач';
  }
  onChatDeleted(chatId: string): void {
    this.chats = this.chats.filter(c => c.id !== chatId);

    if (this.selectedChat?.id === chatId) {
      this.selectedChat = null;
      this.messages = [];
      this.isSidebarOpen = true;
    }
  }

}
