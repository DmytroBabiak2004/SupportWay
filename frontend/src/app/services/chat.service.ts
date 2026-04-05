import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment.development';
import { ChatListItem } from '../models/chat.model';

export type MessageType = 'text' | 'sharedPost' | 'sharedHelpRequest';

export interface SharedPreview {
  id: string;
  entityType: 'post' | 'helpRequest';
  authorUserName: string;
  authorPhotoBase64?: string | null;
  title?: string | null;
  content: string;
  imageBase64?: string | null;
  createdAt: string;
}

export interface Message {
  id: string;
  chatId: string;
  senderId: string;
  content: string;
  sentAt: string;
  isRead?: boolean;

  messageType: number;
  sharedPostId?: string | null;
  sharedHelpRequestId?: string | null;
  sharedPreview?: SharedPreview | null;
}

export interface ReadReceiptEvent {
  chatId: string;
  userId: string;
  lastReadMessageId: string;
}

export interface MessageDeletedEvent {
  chatId: string;
  messageId: string;
  deletedBy: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService implements OnDestroy {
  private baseUrl = `${environment.apiUrl}/chat`;
  private msgUrl  = `${environment.apiUrl}/chat`;
  private hubUrl  = `${environment.apiUrl.replace('/api', '')}/chatHub`;

  private hubConnection: signalR.HubConnection | null = null;

  public connectionStatus$ = new BehaviorSubject<boolean>(false);
  public messageReceived$  = new Subject<Message>();
  public typingEvent$      = new Subject<{ chatId: string; userId: string }>();
  public readReceipt$      = new Subject<ReadReceiptEvent>();
  public messageDeleted$   = new Subject<MessageDeletedEvent>();

  constructor(private http: HttpClient, private auth: AuthService) {}

  getChats(): Observable<ChatListItem[]> {
    return this.http.get<ChatListItem[]>(this.baseUrl);
  }

  getMessages(chatId: string): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.msgUrl}/${chatId}/messages`);
  }

  createOrGetChat(user1Id: string, user2Id: string): Observable<ChatListItem> {
    return this.http.post<ChatListItem>(`${this.baseUrl}/create`, { user1Id, user2Id });
  }

  deleteChat(chatId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${chatId}`);
  }

  sharePostHttp(chatId: string, postId: string, caption?: string): Observable<Message> {
    return this.http.post<Message>(`${this.baseUrl}/share/post`, {
      chatId,
      postId,
      caption: caption ?? ''
    });
  }

  shareHelpRequestHttp(chatId: string, helpRequestId: string, caption?: string): Observable<Message> {
    return this.http.post<Message>(`${this.baseUrl}/share/help-request`, {
      chatId,
      helpRequestId,
      caption: caption ?? ''
    });
  }

  async startSignalR(): Promise<void> {
    if (
      this.hubConnection?.state === signalR.HubConnectionState.Connected ||
      this.hubConnection?.state === signalR.HubConnectionState.Connecting
    ) return;

    const token = localStorage.getItem('auth_token');
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => localStorage.getItem('auth_token') || '',
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.registerServerEvents();

    try {
      await this.hubConnection.start();
      this.connectionStatus$.next(true);
      this.hubConnection.onreconnected(() => this.connectionStatus$.next(true));
      this.hubConnection.onclose(() => this.connectionStatus$.next(false));
    } catch (err) {
      console.error('SignalR Error:', err);
      this.connectionStatus$.next(false);
    }
  }

  private registerServerEvents() {
    if (!this.hubConnection) return;

    this.hubConnection.on('receiveMessage', (message: Message) => {
      this.messageReceived$.next(message);
    });

    this.hubConnection.on('typing', (fromUserId: string, chatId: string) => {
      this.typingEvent$.next({ chatId, userId: fromUserId });
    });

    this.hubConnection.on('chatSeen', (data: ReadReceiptEvent) => {
      this.readReceipt$.next(data);
    });

    this.hubConnection.on('messageDeleted', (data: MessageDeletedEvent) => {
      this.messageDeleted$.next(data);
    });
  }

  async sendMessage(chatId: string, content: string): Promise<void> {
    await this.ensureConnection();
    await this.hubConnection!.invoke('SendMessage', chatId, content);
  }

  async sharePost(chatId: string, postId: string, caption?: string): Promise<void> {
    await this.ensureConnection();
    await this.hubConnection!.invoke('SharePost', chatId, postId, caption ?? '');
  }

  async shareHelpRequest(chatId: string, helpRequestId: string, caption?: string): Promise<void> {
    await this.ensureConnection();
    await this.hubConnection!.invoke('ShareHelpRequest', chatId, helpRequestId, caption ?? '');
  }

  async sendTyping(chatId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('Typing', chatId);
    }
  }

  async markAsRead(chatId: string, lastReadMessageId: string): Promise<void> {
    await this.ensureConnection();
    try {
      await this.hubConnection!.invoke('Seen', chatId, lastReadMessageId);
    } catch {}
  }

  async deleteMessage(messageId: string): Promise<void> {
    await this.ensureConnection();
    await this.hubConnection!.invoke('DeleteMessage', messageId);
  }

  stopConnection() {
    this.hubConnection?.stop().then(() => this.connectionStatus$.next(false));
  }

  getMessageTypeName(messageType: number): MessageType {
    switch (messageType) {
      case 1: return 'sharedPost';
      case 2: return 'sharedHelpRequest';
      default: return 'text';
    }
  }

  getPreviewImageSrc(base64?: string | null): string | null {
    if (!base64?.trim()) return null;
    if (base64.startsWith('data:image/')) return base64;
    return `data:image/jpeg;base64,${base64}`;
  }

  private async ensureConnection(): Promise<void> {
    if (
      !this.hubConnection ||
      this.hubConnection.state === signalR.HubConnectionState.Disconnected
    ) {
      await this.startSignalR();
    }

    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR is not connected');
    }
  }

  ngOnDestroy() {
    this.stopConnection();
  }
}
