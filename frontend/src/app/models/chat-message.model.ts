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

export interface ChatMessage {
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
