import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { Message } from '../../services/chat.service';

@Injectable({ providedIn: 'root' })
export class ChatGatewayService {

  private hub!: signalR.HubConnection;

  // Потік повідомлень (incomingMessages$)
  private incomingMessages = new BehaviorSubject<Message | null>(null);
  incomingMessages$ = this.incomingMessages.asObservable();

  constructor(private auth: AuthService) {}

  // ======================= CONNECT =======================
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
      await this.hub.invoke("RegisterUser", userId);

      console.log("SignalR connected:", userId);

      this.setupListeners();
    }
    catch (e) {
      console.error("SignalR connection failed:", e);
    }
  }

  // ======================= LISTENERS =======================
  private setupListeners() {

    // ReceiveMessage(chatId, senderId, text, createdAt)
    this.hub.on("receiveMessage", (chatId: string, senderId: string, text: string, createdAt: string) => {

      const msg: Message = {
        chatId,
        senderId,
        text,
        createdAt
      };

      this.incomingMessages.next(msg);
    });
  }

  // ======================= JOIN CHAT =======================
  joinChat(chatId: string) {
    if (!this.hub || this.hub.state !== signalR.HubConnectionState.Connected) {
      console.warn("joinChat skipped — hub not connected");
      return;
    }

    this.hub.invoke("JoinChat", chatId);
  }

  // ======================= SEND MESSAGE =======================
  sendMessage(chatId: string, senderId: string, text: string) {

    if (!this.hub || this.hub.state !== signalR.HubConnectionState.Connected) {
      console.warn("sendMessage skipped — hub not connected");
      return;
    }

    return this.hub.invoke("SendMessage", {
      chatId,
      senderId,
      text
    });
  }

  // ======================= TYPING =======================
  sendTyping(chatId: string, fromUserId: string) {

    if (!this.hub || this.hub.state !== signalR.HubConnectionState.Connected) {
      console.warn("Typing skipped — hub not connected");
      return;
    }

    return this.hub.invoke("Typing", chatId, fromUserId);
  }
}
