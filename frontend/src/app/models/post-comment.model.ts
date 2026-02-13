export interface PostComment {
  id: string;
  userId: string;
  userName: string;
  userPhotoBase64?: string; // робимо optional, бо може бути пусто
  text: string;
  createdAt: string;
}

export interface CreatePostCommentDto {
  postId: string;
  requestId?: string;
  text: string;
}
