import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import {UserSearch} from '../models/user-search.model';


@Injectable({ providedIn: 'root' })
export class UserSearchService {
  private apiUrl = `${environment.apiUrl}/users/search`;

  constructor(private http: HttpClient) {}

  searchUsers(name: string): Observable<UserSearch[]> {
    return this.http.get<UserSearch[]>(`${this.apiUrl}?name=${name}`);
  }
}
