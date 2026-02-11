// src/app/services/post.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post } from '../models/post.model';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class PostService {
  private http = inject(HttpClient);
  // Краще брати URL з environment, як у профілі
  private apiUrl = `${environment.apiUrl}/Posts`;

  getFeed(page = 1, size = 10): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.apiUrl}/feed?page=${page}&size=${size}`);
  }

  getMyPosts(page = 1, size = 10): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.apiUrl}/user?page=${page}&size=${size}`);
  }

  // Оновлений метод створення
  createPost(title: string, content: string, imageFile?: File): Observable<void> {
    const formData = new FormData();

    formData.append('title', title);
    formData.append('content', content);

    if (imageFile) {
      // Важливо: 'image' має збігатися з назвою параметру в контролері C# ([FromForm] IFormFile image)
      // АБО якщо ти мапиш це вручну в DTO на бекенді.
      formData.append('image', imageFile);
    }

    return this.http.post<void>(this.apiUrl, formData);
  }

  // Допоміжний метод для відображення картинки з byte[] (Base64)
  getPostImageSrc(base64Image?: string): string | null {
    if (!base64Image) return null;
    // Якщо бекенд віддає чистий base64 без префікса
    return `data:image/jpeg;base64,${base64Image}`;
  }
}
