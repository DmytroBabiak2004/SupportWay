export interface ChatListItem {
  id: string;
  displayName: string;

  userName?: string | null;
  userId?: string | null;

  otherUserId?: string;
  otherUserPhotoBase64?: string | null;
  otherUserIsVerified?: boolean;
  otherUserVerifiedAs?: number | null;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
  isPrivate: boolean;
}

// Keep old Chat interface for backward compatibility with legacy code
export interface Chat {
  id: string;
  name?: string;
  userChats?: {
    userId: string;
    user: { userName: string };
  }[];
}
