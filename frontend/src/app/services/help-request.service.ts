import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { HelpRequest, LocationOption, RequestStatusOption } from '../models/help-request.model';

@Injectable({ providedIn: 'root' })
export class HelpRequestService {
  private http = inject(HttpClient);

  // URL-адреси до контролерів
  private apiUrl = `${environment.apiUrl}/HelpRequests`;
  private locationsUrl = `${environment.apiUrl}/Locations`;
  private statusesUrl = `${environment.apiUrl}/RequestStatuses`; // Переконайтесь, що такий контролер є

  // --- Отримання запитів ---

  getMyRequests(page = 1, size = 10): Observable<HelpRequest[]> {
    return this.http.get<HelpRequest[]>(`${this.apiUrl}/my?page=${page}&size=${size}`);
  }

  getById(id: string): Observable<HelpRequest> {
    return this.http.get<HelpRequest>(`${this.apiUrl}/${id}`);
  }

  // --- Довідники (для форми створення) ---

  getAllLocations(): Observable<LocationOption[]> {
    return this.http.get<LocationOption[]>(this.locationsUrl);
  }

  getAllStatuses(): Observable<RequestStatusOption[]> {
    return this.http.get<RequestStatusOption[]>(this.statusesUrl); // Або RequestStatusesController
  }

  // --- Створення (FormData) ---

  create(
    title: string,
    content: string,
    locationId: string,
    statusId: string,
    imageFile?: File
  ): Observable<void> {
    const formData = new FormData();

    // Ключі повинні збігатися з властивостями DTO на бекенді (case-insensitive, але краще точно)
    formData.append('title', title);
    formData.append('content', content);
    formData.append('locationId', locationId);
    formData.append('requestStatusId', statusId);

    if (imageFile) {
      formData.append('image', imageFile);
    }

    return this.http.post<void>(this.apiUrl, formData);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
