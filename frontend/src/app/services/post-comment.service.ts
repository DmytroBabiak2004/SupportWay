import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { PostComment, CreatePostCommentDto } from '../models/post-comment.model';

@Injectable({ providedIn: 'root' })
export class PostCommentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/PostComments`;

  getCommentsByPost(postId: string) {
    return this.http.get<PostComment[]>(`${this.apiUrl}/post/${postId}`);
  }

  addComment(dto: CreatePostCommentDto) {
    return this.http.post(`${this.apiUrl}`, dto);
  }

  deleteComment(commentId: string) {
    return this.http.delete(`${this.apiUrl}/${commentId}`);
  }
}
