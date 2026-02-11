import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class RatingService {
  private apiUrl = `${environment.apiUrl}/rating`;

  constructor(private http: HttpClient) {}

  rateProfile(profileId: string, value: number): Observable<{ averageRating: number }> {
    return this.http.post<{ averageRating: number }>(this.apiUrl, {
      profileId: profileId,
      value: value
    });
  }
}
