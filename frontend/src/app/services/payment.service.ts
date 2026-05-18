import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { DonateResponseDto } from '../models/map.models';

export interface DonateRequest {
  helpRequestId: string;
  amount: number;
  comment: string;
}

export interface PaymentRecord {
  paymentId: string;
  status: string;
  provider?: string | null;
  amount: number;
  helpRequestId?: string | null;
  createdAt: string;
  comment?: string | null;
}

export interface JarInfo {
  title?: string | null;
  description?: string | null;
  amountUah: number;
  goalUah: number;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http    = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/payments`;

  donate(payload: DonateRequest): Observable<DonateResponseDto> {
    return this.http.post<DonateResponseDto>(`${this.baseUrl}/donate`, payload);
  }

  getPaymentStatus(paymentId: string): Observable<PaymentRecord> {
    return this.http.get<PaymentRecord>(`${this.baseUrl}/${paymentId}`);
  }

  getMyPayments(): Observable<PaymentRecord[]> {
    return this.http.get<PaymentRecord[]>(`${this.baseUrl}/my`);
  }

  getJarInfo(jarId: string): Observable<JarInfo> {
    return this.http.get<JarInfo>(`${this.baseUrl}/jar-info/${jarId}`);
  }
}
