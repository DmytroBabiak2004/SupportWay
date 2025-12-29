// user-search.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserSearch {
  id: string;
  userName: string;
}

@Injectable({ providedIn: 'root' })
export class UserSearchService {
  private apiUrl = 'http://localhost:5233/api/users/search';

  constructor(private http: HttpClient) {}

  searchUsers(name: string): Observable<UserSearch[]> {
    return this.http.get<UserSearch[]>(`${this.apiUrl}?name=${name}`);
  }
}
