import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { HelpRequest } from '../models/help-request.model';

export interface LocationPayload {
  locationId?: string;
  latitude?: number;
  longitude?: number;
  address?: string;
  districtName?: string;
}

@Injectable({ providedIn: 'root' })
export class HelpRequestService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/HelpRequests`;

  getFeed(page = 1, size = 10): Observable<HelpRequest[]> {
    return this.http.get<HelpRequest[]>(`${this.apiUrl}/feed?page=${page}&size=${size}`);
  }

  getMyHelpRequests(page = 1, size = 10): Observable<HelpRequest[]> {
    return this.http.get<HelpRequest[]>(`${this.apiUrl}/my?page=${page}&size=${size}`);
  }

  getById(id: string): Observable<HelpRequest> {
    return this.http.get<HelpRequest>(`${this.apiUrl}/${id}`);
  }

  createHelpRequest(
    title: string,
    content: string,
    location?: LocationPayload,
    imageFile?: File
  ): Observable<void> {
    const formData = new FormData();
    formData.append('title', title);
    formData.append('content', content);

    if (location) {
      if (location.locationId) {
        formData.append('locationId', location.locationId);
      }
      if (location.latitude != null) {
        formData.append('latitude', location.latitude.toString());
      }
      if (location.longitude != null) {
        formData.append('longitude', location.longitude.toString());
      }
      if (location.address) {
        formData.append('address', location.address);
      }
      if (location.districtName) {
        formData.append('districtName', location.districtName);
      }
    }

    if (imageFile) {
      formData.append('image', imageFile);
    }

    return this.http.post<void>(this.apiUrl, formData);
  }

  getHelpRequestImageSrc(base64Image?: string | null): string | null {
    if (!base64Image) return null;
    return `data:image/jpeg;base64,${base64Image}`;
  }
}

