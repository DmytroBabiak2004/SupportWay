import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Notification, NotificationType } from '../../../models/notification.model';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-notification-item',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-item.component.html',
  styleUrls: ['./notification-item.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationItemComponent {
  @Input() notification!: Notification;
  @Output() notificationClick = new EventEmitter<void>();

  readonly NotificationType = NotificationType;

  constructor(private notificationService: NotificationService) {}

  get imageSrc(): string | null {
    return this.notificationService.getImageSrc(this.notification.imageBase64);
  }

  get relativeTime(): string {
    const diff = Date.now() - new Date(this.notification.createdAt).getTime();
    const minutes = Math.floor(diff / 60_000);
    if (minutes < 1) return 'щойно';
    if (minutes < 60) return `${minutes} хв тому`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} год тому`;
    const days = Math.floor(hours / 24);
    return `${days} д тому`;
  }

  onClick(): void {
    this.notificationClick.emit();
  }
}
