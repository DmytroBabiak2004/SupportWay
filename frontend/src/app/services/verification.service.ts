import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { SubmitVerificationDto, VerificationRequest, VerificationStatus } from '../models/verification.model';

@Injectable({ providedIn: 'root' })
export class VerificationService {
  private apiUrl = `${environment.apiUrl}/verification`;
  constructor(private http: HttpClient) {}

  getMyRequest(): Observable<VerificationRequest | null> {
    return this.http.get<VerificationRequest | null>(`${this.apiUrl}/my`);
  }

  submit(dto: SubmitVerificationDto): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, dto);
  }

  // Admin
  getAll(status?: VerificationStatus): Observable<VerificationRequest[]> {
    let params = new HttpParams();
    if (status !== undefined) params = params.set('status', status);
    return this.http.get<VerificationRequest[]>(`${this.apiUrl}/admin`, { params });
  }

  decide(id: string, approved: boolean, adminComment?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/admin/${id}/decide`, { approved, adminComment });
  }
}
