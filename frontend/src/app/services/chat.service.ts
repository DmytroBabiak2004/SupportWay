// chat.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

export interface ChatUser {
  id: string;
  userName: string;
}

export interface Message {
  chatId: string;
  senderId: string;
  text: string;
  createdAt: string;
}

export interface Chat {
  id: string;
  name: string;
  startedAt: string;
  userChats?: {
    userId: string;
    user: ChatUser;
  }[];
  messages?: Message[];
}

export interface CreateChatRequest {
  user1Id: string;
  user2Id: string;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private baseUrl = 'http://localhost:5233/api/chat';

  constructor(
    private http: HttpClient,
    private auth: AuthService
  ) {}

  private headers(): HttpHeaders {
    return new HttpHeaders({
      'Authorization': `Bearer ${this.auth.getToken()}`,
      'Content-Type': 'application/json'
    });
  }

  getUserChats(): Observable<Chat[]> {
    return this.http.get<Chat[]>(this.baseUrl, {
      headers: this.headers()
    });
  }

  getChatById(chatId: string): Observable<Chat> {
    return this.http.get<Chat>(`${this.baseUrl}/${chatId}`, {
      headers: this.headers()
    });
  }

  createChat(data: CreateChatRequest): Observable<Chat> {
    return this.http.post<Chat>(
      `${this.baseUrl}/create`,
      data,
      { headers: this.headers() }
    );
  }

  deleteChat(chatId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/${chatId}`,
      { headers: this.headers() }
    );
  }
}
