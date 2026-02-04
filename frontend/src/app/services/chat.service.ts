import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { AuthService } from './auth.service';

export interface Message {
  chatId: string;
  senderId: string;
  content: string;
  sentAt: string;
}

export interface Chat {
  id: string;
  name?: string;
  userChats?: {
    userId: string;
    user: { userName: string };
  }[];
}

@Injectable({ providedIn: 'root' })
export class ChatService implements OnDestroy {
  private baseUrl = 'http://localhost:5233/api/chat';
  private hubConnection!: signalR.HubConnection;

  // Стан підключення для UI (наприклад, показувати зелений індикатор)
  public connectionStatus$ = new BehaviorSubject<boolean>(false);

  public messageReceived$ = new Subject<Message>();
  public typingEvent$ = new Subject<{ chatId: string, userId: string }>();

  constructor(private http: HttpClient, private auth: AuthService) {}

  // --- HTTP Методи ---

  getChats(): Observable<Chat[]> {
    return this.http.get<Chat[]>(this.baseUrl);
  }

  getMessages(chatId: string): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.baseUrl}/${chatId}/messages`);
  }

  createChat(user1Id: string, user2Id: string): Observable<Chat> {
    return this.http.post<Chat>(`${this.baseUrl}/create`, {
      user1Id: user1Id, // Велика літера, якщо в C# так само
      user2Id: user2Id
    });
  }


  async startSignalR() {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) return;
    const token = localStorage.getItem('auth_token') || '';

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5233/chatHub', {
        accessTokenFactory: () => localStorage.getItem('auth_token') || ''
        // Прибираємо skipNegotiation та transport на час тесту
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information) // Trace занадто детальний зараз
      .build();

    this.registerServerEvents();

    try {
      await this.hubConnection.start();
      console.log('SignalR: Connected');
      this.connectionStatus$.next(true);
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      this.connectionStatus$.next(false);
    }
  }

  private registerServerEvents() {
    this.hubConnection.on('receiveMessage', (messageId: string, fromUserId: string, content: string, chatId: string, sentAt: string) => {
      this.messageReceived$.next({ chatId, senderId: fromUserId, content, sentAt: sentAt });
    });

    this.hubConnection.on('typing', (fromUserId: string, chatId: string) => {
      this.typingEvent$.next({ chatId, userId: fromUserId });
    });
  }

  async sendMessage(chatId: string, text: string) {
    if (this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Неможливо надіслати повідомлення: з’єднання відсутнє');
    }
    // Назва методу 'SendMessage' має бути такою ж, як у C# класі Hub
    await this.hubConnection.invoke('SendMessage', chatId, text);
  }

  async sendTyping(chatId: string) {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('Typing', chatId);
    }
  }

  stopConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR: Connection stopped'))
        .catch(err => console.error('SignalR Stop Error: ', err));
    }
  }

  ngOnDestroy() {
    this.stopConnection();
  }
  deleteChat(chatId: string) {
    return this.http.delete(`${this.baseUrl}/chat/${chatId}`);
  }
}

