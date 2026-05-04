import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment.development';
import { FaqBotResponse, FaqSuggestion } from '../models/faq-bot.model';

@Injectable({ providedIn: 'root' })
export class FaqBotService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/FaqBot`;

  ask(question: string): Observable<FaqBotResponse> {
    return this.http.post<FaqBotResponse>(`${this.apiUrl}/ask`, { question });
  }

  getSuggestions(): Observable<FaqSuggestion[]> {
    return this.http.get<FaqSuggestion[]>(`${this.apiUrl}/suggestions`);
  }
}
