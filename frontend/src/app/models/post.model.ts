export interface Post {
  id: string;
  title: string;
  content: string;
  image?: string;
  createdAt: string;
  userId: string;
  likesCount: number;
  isLikedByCurrentUser: boolean;

  authorUserName?: string;
  authorName?: string;
  authorFullName?: string;
  authorPhotoBase64?: string;
  authorIsVerified?: boolean;
  authorVerifiedAs?: number | null;

  commentsCount?: number;
}
export interface CreatePostDto {
  title: string;
  content: string;
  image?: File;
}
