import {
  Component, OnInit, OnDestroy, HostListener, ChangeDetectionStrategy, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { NotificationService } from '../../../services/notification.service';
import { NotificationItemComponent } from '../notification-item/notification-item.component';
import { Notification, NotificationType } from '../../../models/notification.model';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, NotificationItemComponent],
  templateUrl: './notification-bell.component.html',
  styleUrls: ['./notification-bell.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationBellComponent implements OnInit, OnDestroy {

  isOpen = false;
  isLoading = false;
  unreadCount = 0;
  notifications: Notification[] = [];

  private subscriptions = new Subscription();

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  trackById(index: number, item: Notification): string {
    return item.id;
  }

  ngOnInit(): void {
    this.subscriptions.add(
      this.notificationService.unreadCount$.subscribe(count => {
        this.unreadCount = count;
        this.cdr.markForCheck();
      })
    );

    this.subscriptions.add(
      this.notificationService.notifications$.subscribe(items => {
        this.notifications = items;
        this.cdr.markForCheck();
      })
    );
  }

  toggleDropdown(event: Event): void {
    event.stopPropagation();
    this.isOpen = !this.isOpen;

    if (this.isOpen && this.notifications.length === 0) {
      this.fetchNotifications();
    }
  }

  private fetchNotifications(): void {
    this.isLoading = true;
    this.cdr.markForCheck();

    this.notificationService.loadNotifications(1, 20).subscribe({
      next: response => {
        this.notificationService.notifications$.next(response.items);
        this.notificationService.unreadCount$.next(response.unreadCount);
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  onNotificationClick(notification: Notification): void {
    if (!notification.isRead) {
      this.notificationService.markAsReadViaHub(notification.id).catch(() => {
        // fall back to HTTP if hub not connected
        this.notificationService.markAsRead(notification.id).subscribe();
      });

      // Optimistic update
      const updated = this.notifications.map(n =>
        n.id === notification.id ? { ...n, isRead: true } : n
      );
      this.notificationService.notifications$.next(updated);
    }

    this.isOpen = false;
    this.navigate(notification);
  }

  private navigate(notification: Notification): void {
    if (notification.type === NotificationType.Message && notification.relatedEntityId) {
      this.router.navigate(['/chat'], {
        queryParams: { chatId: notification.relatedEntityId }
      });
    } else if (notification.type === NotificationType.BadgeAwarded) {
      this.router.navigate(['/profile'], { fragment: 'badges' });
    }
  }

  markAllAsRead(event: Event): void {
    event.stopPropagation();
    this.notificationService.markAllAsReadViaHub().catch(() => {
      this.notificationService.markAllAsRead().subscribe();
    });

    const cleared = this.notifications.map(n => ({ ...n, isRead: true }));
    this.notificationService.notifications$.next(cleared);
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    if (this.isOpen) {
      this.isOpen = false;
      this.cdr.markForCheck();
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }
}
