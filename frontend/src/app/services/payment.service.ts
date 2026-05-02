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

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/payments`;

  donate(payload: DonateRequest): Observable<DonateResponseDto> {
    return this.http.post<DonateResponseDto>(`${this.baseUrl}/donate`, payload);
  }

  getPaymentStatus(paymentId: string): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${paymentId}`);
  }
}
