import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { HelpRequest, HelpRequestDetails } from '../models/help-request.model';

export interface LocationPayload {
  locationId?: string;
  latitude?: number;
  longitude?: number;
  address?: string;
  districtName?: string;
}

@Injectable({ providedIn: 'root' })
export class HelpRequestService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/HelpRequests`;

  getFeed(page = 1, size = 10): Observable<HelpRequest[]> {
    return this.http.get<HelpRequest[]>(`${this.apiUrl}/feed?page=${page}&size=${size}`);
  }

  getMyHelpRequests(page = 1, size = 10): Observable<HelpRequest[]> {
    return this.http.get<HelpRequest[]>(`${this.apiUrl}/my?page=${page}&size=${size}`);
  }

  getById(id: string): Observable<HelpRequest> {
    return this.http.get<HelpRequest>(`${this.apiUrl}/${id}`);
  }

  getDetails(id: string): Observable<HelpRequestDetails> {
    return this.http.get<HelpRequestDetails>(`${this.apiUrl}/${id}/details`);
  }

  createHelpRequest(title: string, content: string, location?: LocationPayload, imageFile?: File): Observable<void> {
    const formData = new FormData();
    formData.append('title', title);
    formData.append('content', content);

    if (location) {
      if (location.locationId) formData.append('locationId', location.locationId);
      if (location.latitude != null) formData.append('latitude', location.latitude.toString());
      if (location.longitude != null) formData.append('longitude', location.longitude.toString());
      if (location.address) formData.append('address', location.address);
      if (location.districtName) formData.append('districtName', location.districtName);
    }

    if (imageFile) formData.append('image', imageFile);
    return this.http.post<void>(this.apiUrl, formData);
  }

  createHelpRequestDirect(data: any): Observable<any> {
    const formData = new FormData();
    formData.append('title', data.title);
    formData.append('content', data.content);
    if (data.address) formData.append('address', data.address);
    if (data.districtName) formData.append('districtName', data.districtName);
    if (data.file) formData.append('image', data.file);
    if (data.preferredDonationMethod) formData.append('preferredDonationMethod', data.preferredDonationMethod);
    if (data.donationRecipientName) formData.append('donationRecipientName', data.donationRecipientName);
    if (data.donationRecipientCardNumber) formData.append('donationRecipientCardNumber', data.donationRecipientCardNumber);
    if (data.donationRecipientIban) formData.append('donationRecipientIban', data.donationRecipientIban);
    if (data.donationPaymentLink) formData.append('donationPaymentLink', data.donationPaymentLink);
    if (data.donationNotes) formData.append('donationNotes', data.donationNotes);

    let params = new HttpParams();
    if (data.latitude != null) params = params.set('latitude', Number(data.latitude).toFixed(7));
    if (data.longitude != null) params = params.set('longitude', Number(data.longitude).toFixed(7));

    return this.http.post<any>(this.apiUrl, formData, { params });
  }

  getImageSrc(base64?: string | null): string | null {
    if (!base64) return null;
    return `data:image/jpeg;base64,${base64}`;
  }
}
