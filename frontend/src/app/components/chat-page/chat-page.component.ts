import { Component, OnInit } from '@angular/core';
import { ChatService, Chat, Message } from '../../services/chat.service';
import { AuthService } from '../../services/auth.service';
import { ChatGatewayService } from '../../services/chat-gateway-service';
import { FormsModule } from '@angular/forms';
import { DatePipe, NgForOf, NgIf } from '@angular/common';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  templateUrl: './chat-page.component.html',
  styleUrls: ['./chat-page.component.scss'],
  imports: [FormsModule, NgForOf, NgIf, DatePipe]
})
export class ChatPageComponent implements OnInit {

  chats: Chat[] = [];
  selectedChat: Chat | null = null;

  messages: Message[] = [];
  newMessage = '';

  userId!: string;
  typingUser: string | null = null;

  constructor(
    private chatService: ChatService,
    private authService: AuthService,
    private gateway: ChatGatewayService
  ) {}

  async ngOnInit() {
    this.userId = this.authService.getUserId();

    this.loadChats();

    await this.gateway.startConnection();

    this.gateway.incomingMessages$.subscribe((msg: Message | null) => {
      if (!msg || !this.selectedChat) return;

      if (msg.chatId === this.selectedChat.id) {
        this.messages.push(msg);
        this.autoScroll();
      }
    });

    this.gateway.typingEvents$.subscribe((userId: string | null) => {
      if (!userId || !this.selectedChat) return;

      if (userId === this.getOtherUserId()) {
        this.typingUser = userId;
        setTimeout(() => this.typingUser = null, 1200);
      }
    });
  }

  loadChats() {
    this.chatService.getUserChats().subscribe(chats => {
      this.chats = chats;
    });
  }

  openChat(chat: Chat) {
    this.selectedChat = chat;

    this.chatService.getChatById(chat.id).subscribe(full => {
      this.messages = full.messages ?? [];
      setTimeout(() => this.autoScroll(), 50);
    });
  }

  sendMessage() {
    if (!this.newMessage.trim() || !this.selectedChat) return;

    this.gateway.sendMessage(
      this.selectedChat.id,
      this.userId,
      this.newMessage
    );

    const localMsg: Message = {
      chatId: this.selectedChat.id,
      senderId: this.userId,
      text: this.newMessage,
      createdAt: new Date().toISOString()
    };

    this.messages.push(localMsg);
    this.newMessage = '';
    this.autoScroll();
  }

  onTyping() {
    if (!this.selectedChat) return;

    const receiver = this.getOtherUserId();
    if (!receiver) return;

    this.gateway.sendTyping(this.userId, receiver);
  }

  getOtherUserId(): string {
    if (!this.selectedChat?.userChats) return '';

    const other = this.selectedChat.userChats
      .find(uc => uc.user.id !== this.userId);

    return other?.user.id ?? '';
  }

  autoScroll() {
    const box = document.getElementById("scrollBox");
    if (box) {
      box.scrollTop = box.scrollHeight;
    }
  }
}
