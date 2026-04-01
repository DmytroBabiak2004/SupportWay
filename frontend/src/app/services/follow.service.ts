import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { FollowUser } from '../models/follow-user.model';

@Injectable({ providedIn: 'root' })
export class FollowService {
  private apiUrl = `${environment.apiUrl}/follow`;
  constructor(private http: HttpClient) {}

  follow(followedId: string): Observable<{ followersCount: number }> {
    return this.http.post<any>(`${this.apiUrl}/${followedId}`, {});
  }
  unfollow(followedId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${followedId}`);
  }
  isFollowing(followedId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/is-following/${followedId}`);
  }
  getFollowersCount(userId: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${userId}/followers-count`);
  }
  getFollowingCount(userId: string): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/${userId}/following-count`);
  }
  getFollowers(userId: string): Observable<FollowUser[]> {
    return this.http.get<FollowUser[]>(`${this.apiUrl}/${userId}/followers`);
  }
  getFollowing(userId: string): Observable<FollowUser[]> {
    return this.http.get<FollowUser[]>(`${this.apiUrl}/${userId}/following`);
  }
  removeFollower(followerUserId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/remove-follower/${followerUserId}`);
  }
}
