// chat-gateway-service.ts
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from './auth.service';
import { Message } from './chat.service';

@Injectable({ providedIn: 'root' })
export class ChatGatewayService {

  private hub!: signalR.HubConnection;

  private incomingMessages = new BehaviorSubject<Message | null>(null);
  incomingMessages$ = this.incomingMessages.asObservable();

  private typingEvents = new BehaviorSubject<string | null>(null);
  typingEvents$ = this.typingEvents.asObservable();

  constructor(private auth: AuthService) {}

  async startConnection(): Promise<void> {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5233/chatHub', {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    try {
      await this.hub.start();

      const userId = this.auth.getUserId();
      await this.hub.invoke('RegisterUser', userId);

      console.log('SignalR connected as:', userId);

      this.setupListeners();
    } catch (e) {
      console.error('SignalR connection failed:', e);
    }
  }

  private setupListeners() {
    this.hub.on(
      'receiveMessage',
      (fromUserId: string, text: string, chatId: string, createdAt: string) => {
        const msg: Message = {
          chatId,
          senderId: fromUserId,
          text,
          createdAt
        };

        this.incomingMessages.next(msg);
      }
    );

    this.hub.on('typing', (fromUserId: string) => {
      this.typingEvents.next(fromUserId);
    });
  }

  sendMessage(chatId: string, fromUserId: string, text: string) {
    if (!this.hub || this.hub.state !== signalR.HubConnectionState.Connected) {
      console.warn('sendMessage skipped — hub not connected');
      return;
    }

    return this.hub.invoke('SendMessage', chatId, fromUserId, text);
  }

  sendTyping(fromUserId: string, toUserId: string) {
    if (!this.hub || this.hub.state !== signalR.HubConnectionState.Connected) {
      console.warn('Typing skipped — hub not connected');
      return;
    }

    return this.hub.invoke('Typing', fromUserId, toUserId);
  }
}
