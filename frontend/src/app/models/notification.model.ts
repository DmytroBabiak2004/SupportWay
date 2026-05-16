export enum NotificationType {
  Message = 0,
  BadgeAwarded = 1,
  System = 2
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
  relatedEntityId?: string | null;
  relatedEntityType?: string | null;
  imageBase64?: string | null;
}

export interface NotificationPagedResponse {
  items: Notification[];
  unreadCount: number;
  page: number;
  pageSize: number;
}
