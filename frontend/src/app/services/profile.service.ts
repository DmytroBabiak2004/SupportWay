import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Profile } from '../models/profile.model';
import {environment} from '../../environments/environment.development';


@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private apiUrl = `${environment.apiUrl}/Profile`;
  constructor(private http: HttpClient) {}

  getProfile(userId?: string): Observable<Profile> {
    // Якщо userId передано і це не рядок 'me' (про всяк випадок)
    if (userId && userId !== 'me') {
      return this.http.get<Profile>(`${this.apiUrl}/profiles/${userId}`);
    }
    return this.http.get<Profile>(`${this.apiUrl}/profiles/me`);
  }
  updateDescription(description: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/description`, {
      description: description
    });
  }

  updatePhoto(file: File): Observable<void> {
    const formData = new FormData();
    formData.append('photo', file);
    return this.http.put<void>(`${this.apiUrl}/photo`, formData);
  }

  getAvatarSrc(base64?: string): string {
    if (base64) {
      if (base64.startsWith('data:image')) return base64;
      return `data:image/jpeg;base64,${base64}`;
    }
    return 'assets/images/default-avatar.png';
  }
}
