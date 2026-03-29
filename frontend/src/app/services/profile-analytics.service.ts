import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { ProfileAnalytics } from '../models/profile-analytics.model';

@Injectable({
  providedIn: 'root'
})
export class ProfileAnalyticsService {
  private apiUrl = `${environment.apiUrl}/ProfileAnalytics`;

  constructor(private http: HttpClient) {}

  getDashboard(profileId: string): Observable<ProfileAnalytics> {
    return this.http.get<ProfileAnalytics>(`${this.apiUrl}/${profileId}`);
  }
}
