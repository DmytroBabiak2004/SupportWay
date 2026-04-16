import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Badge } from '../models/profile-badge.model';

@Injectable({
  providedIn: 'root'
})
export class BadgeService {
  private apiUrl = `${environment.apiUrl}/Badges`;
  private profileBadgesUrl = `${environment.apiUrl}/ProfileBadges`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Badge[]> {
    return this.http.get<Badge[]>(this.apiUrl);
  }

  getByProfileId(profileId: string): Observable<Badge[]> {
    return this.http.get<Badge[]>(`${this.apiUrl}/profile/${profileId}`);
  }

  getById(id: string): Observable<Badge> {
    return this.http.get<Badge>(`${this.apiUrl}/${id}`);
  }

  removeFromProfile(profileId: string, badgeId: string): Observable<void> {
    return this.http.delete<void>(
      `${this.profileBadgesUrl}/profile/${profileId}/badge/${badgeId}`
    );
  }

  awardToProfile(profileId: string, badgeId: string): Observable<void> {
    return this.http.post<void>(this.profileBadgesUrl, { profileId, badgeId });
  }

  getBadgeImageSrc(imageBase64?: string | null): string {
    if (!imageBase64 || !imageBase64.trim()) {
      return this.getFallbackSvg();
    }

    const clean = imageBase64.trim();

    if (clean.startsWith('data:image/')) {
      return clean;
    }

    if (clean.startsWith('iVBOR')) {
      return `data:image/png;base64,${clean}`;
    }

    if (clean.startsWith('/9j/')) {
      return `data:image/jpeg;base64,${clean}`;
    }

    if (clean.startsWith('UklGR')) {
      return `data:image/webp;base64,${clean}`;
    }

    return `data:image/png;base64,${clean}`;
  }

  private getFallbackSvg(): string {
    const svg = `
      <svg xmlns="http://www.w3.org/2000/svg" width="96" height="96" viewBox="0 0 96 96">
        <rect width="96" height="96" rx="24" fill="#1e293b"/>
        <circle cx="48" cy="36" r="14" fill="#64748b"/>
        <rect x="24" y="58" width="48" height="12" rx="6" fill="#475569"/>
      </svg>
    `;

    return `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`;
  }
}
