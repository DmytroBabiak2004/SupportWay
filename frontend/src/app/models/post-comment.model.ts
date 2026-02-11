export interface PostComment {
  id: string;
  userId: string;
  userName: string;
  text: string;
  createdAt: string;
}

export interface CreatePostCommentDto {
  postId: string;
  requestId?: string;
  text: string;
}
