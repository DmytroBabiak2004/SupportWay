import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Notification, NotificationPagedResponse } from '../models/notification.model';
import { ToastService } from './toast.service';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class NotificationService implements OnDestroy {
  private readonly apiUrl = `${environment.apiUrl}/notifications`;
  private readonly hubUrl = `${environment.apiUrl.replace('/api', '')}/notificationHub`;

  private hubConnection: signalR.HubConnection | null = null;
  private connectionPromise: Promise<void> | null = null;

  public unreadCount$ = new BehaviorSubject<number>(0);
  public notifications$ = new BehaviorSubject<Notification[]>([]);
  public connectionStatus$ = new BehaviorSubject<boolean>(false);

  constructor(
    private http: HttpClient,
    private toastService: ToastService,
    private router: Router
  ) {}

  async startConnection(): Promise<void> {
    if (
      this.hubConnection?.state === signalR.HubConnectionState.Connected ||
      this.hubConnection?.state === signalR.HubConnectionState.Connecting
    ) return;

    if (this.connectionPromise) {
      return this.connectionPromise;
    }

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

    this.connectionPromise = this.hubConnection.start()
      .then(() => {
        this.connectionStatus$.next(true);
        this.hubConnection!.onreconnected(() => this.connectionStatus$.next(true));
        this.hubConnection!.onclose(() => this.connectionStatus$.next(false));
        this.connectionPromise = null;
      })
      .catch((err) => {
        console.error('NotificationHub error:', err);
        this.connectionStatus$.next(false);
        this.connectionPromise = null;
        throw err;
      });

    return this.connectionPromise;
  }

  private registerServerEvents(): void {
    if (!this.hubConnection) return;

    this.hubConnection.off('receiveNotification');
    this.hubConnection.off('unreadCountUpdated');

    this.hubConnection.on('receiveNotification', (notification: Notification) => {
      const current = this.notifications$.value;
      this.notifications$.next([notification, ...current]);
      this.unreadCount$.next(this.unreadCount$.value + 1);
    });

    this.hubConnection.on('unreadCountUpdated', (count: number) => {
      this.unreadCount$.next(count);
    });
  }

  stopConnection(): void {
    this.hubConnection?.stop().then(() => this.connectionStatus$.next(false));
  }

  async markAsReadViaHub(notificationId: string): Promise<void> {
    await this.ensureConnection();
    await this.hubConnection!.invoke('MarkAsRead', notificationId);
  }

  async markAllAsReadViaHub(): Promise<void> {
    await this.ensureConnection();
    await this.hubConnection!.invoke('MarkAllAsRead');
  }

  loadNotifications(page = 1, pageSize = 20, unreadOnly?: boolean): Observable<NotificationPagedResponse> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);

    if (unreadOnly !== undefined) {
      params = params.set('unreadOnly', unreadOnly);
    }

    return this.http.get<NotificationPagedResponse>(this.apiUrl, { params });
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.apiUrl}/unread-count`);
  }

  markAsRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/read-all`, {});
  }

  deleteNotification(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getImageSrc(base64?: string | null): string | null {
    if (!base64?.trim()) return null;
    if (base64.startsWith('data:image/')) return base64;
    return `data:image/jpeg;base64,${base64}`;
  }

  private async ensureConnection(): Promise<void> {
    if (
      !this.hubConnection ||
      this.hubConnection.state === signalR.HubConnectionState.Disconnected
    ) {
      await this.startConnection();
    }
    if (this.hubConnection?.state !== signalR.HubConnectionState.Connected) {
      throw new Error('NotificationHub is not connected');
    }
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}
