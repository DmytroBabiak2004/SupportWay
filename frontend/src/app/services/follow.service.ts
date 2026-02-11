import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class FollowService {
  private apiUrl = `${environment.apiUrl}/follow`;

  constructor(private http: HttpClient) {}

  // Підписатися
  follow(followedId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${followedId}`, {});
  }

  // Відписатися
  unfollow(followedId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${followedId}`);
  }

  // Перевірити статус підписки
  isFollowing(followedId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/is-following/${followedId}`);
  }

  // Отримати кількість підписників
  getFollowersCount(userId: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${userId}/followers-count`);
  }

  // Отримати кількість тих, на кого підписаний користувач
  getFollowingCount(userId: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${userId}/following-count`);
  }
}
