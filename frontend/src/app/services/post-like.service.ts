import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import { PostLikeDto } from '../models/post-like.model';

@Injectable({ providedIn: 'root' })
export class PostLikeService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/PostLikes`;

  likePost(dto: PostLikeDto) {
    return this.http.post(`${this.apiUrl}/like`, dto);
  }

  unlikePost(dto: PostLikeDto) {
    return this.http.post(`${this.apiUrl}/unlike`, dto);
  }

  getLikesCount(postId: string) {
    return this.http.get<number>(`${this.apiUrl}/count/${postId}`);
  }
}
