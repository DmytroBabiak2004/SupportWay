import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';

export interface UserInfo {
  id: string;
  username: string;
  roles: string[];
}

interface AuthResponse {
  id: string;
  username: string;
  roles: string[];
  token: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = 'http://localhost:5233/api/Auth';
  private userInfo = new BehaviorSubject<UserInfo | null>(null);

  constructor(private http: HttpClient, private router: Router) {
    const stored = localStorage.getItem('userInfo');
    if (stored) this.userInfo.next(JSON.parse(stored));
  }

  getCurrentUser(): UserInfo | null {
    return this.userInfo.value;
  }

  getUserId(): string {
    return this.userInfo.value?.id ?? '';
  }

  getToken(): string | null {
    return localStorage.getItem('auth_token');
  }

  private saveToken(token: string) {
    localStorage.setItem('auth_token', token);
  }

  login(username: string, password: string): Observable<UserInfo> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, { username, password })
      .pipe(
        tap(res => {
          this.saveToken(res.token);

          const info: UserInfo = {
            id: res.id,
            username: res.username,
            roles: res.roles
          };

          this.userInfo.next(info);
          localStorage.setItem('userInfo', JSON.stringify(info));
        }),
        map(res => ({
          id: res.id,
          username: res.username,
          roles: res.roles
        }))
      );
  }

  register(username: string, password: string, role: string): Observable<UserInfo> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, { username, password, role })
      .pipe(
        tap(res => {
          this.saveToken(res.token);

          const info: UserInfo = {
            id: res.id,
            username: res.username,
            roles: res.roles
          };

          this.userInfo.next(info);
          localStorage.setItem('userInfo', JSON.stringify(info));
        }),
        map(res => ({
          id: res.id,
          username: res.username,
          roles: res.roles
        }))
      );
  }

  checkSession(): Observable<boolean> {
    const token = this.getToken();
    if (!token) return of(false);

    const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });

    return this.http.get<AuthResponse>(`${this.apiUrl}/check-session`, { headers })
      .pipe(
        tap(res => {
          const info: UserInfo = {
            id: res.id,
            username: res.username,
            roles: res.roles
          };
          this.userInfo.next(info);
          localStorage.setItem('userInfo', JSON.stringify(info));
        }),
        map(() => true),
        catchError(() => of(false))
      );
  }

  logout() {
    this.userInfo.next(null);
    localStorage.removeItem('auth_token');
    localStorage.removeItem('userInfo');
    this.router.navigate(['/login']);
  }
}
