import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, of, BehaviorSubject, throwError } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { Router } from '@angular/router';

// Інтерфейс для даних користувача
export interface UserInfo {
  username: string;
  roles: string[];
}

// Інтерфейс для відповіді від check-session
interface CheckSessionResponse {
  message: string;
  username: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '//localhost:5073/api/Auth';
  private userInfo = new BehaviorSubject<UserInfo | null>(null); // Зберігаємо дані користувача

  constructor(private http: HttpClient, private router: Router) {
    // Завантажуємо збережені дані при ініціалізації
    const storedUser = localStorage.getItem('userInfo');
    if (storedUser) {
      this.userInfo.next(JSON.parse(storedUser));
    }
  }

  // Отримання поточних даних користувача
  getCurrentUser(): UserInfo | null {
    return this.userInfo.value;
  }

  // Вхід користувача
  login(username: string, password: string): Observable<any> {
    const body = { username, password };
    return this.http.post(`${this.apiUrl}/login`, body, { headers: { 'Content-Type': 'application/json' } }).pipe(
      tap((response: any) => {
        if (response && response.token) {
          this.saveToken(response.token);
          // Зберігаємо username і roles з відповіді або токена
          const userInfo: UserInfo = {
            username: response.username || username,
            roles: response.roles || this.getUserRolesFromToken(response.token)
          };
          this.userInfo.next(userInfo);
          localStorage.setItem('userInfo', JSON.stringify(userInfo));
          console.log('Токен і дані збережено:', response.token, userInfo);
        } else {
          throw new Error('Токен не отримано від сервера');
        }
      }),
      catchError((err: HttpErrorResponse) => {
        console.error('Помилка входу:', err);
        return throwError(() => new Error(err.error?.message || 'Помилка входу'));
      })
    );
  }

  // Реєстрація користувача
  register(username: string, password: string, role: string): Observable<any> {
    const body = { username, password, role };
    return this.http.post(`${this.apiUrl}/register`, body, { headers: { 'Content-Type': 'application/json' } }).pipe(
      tap((response: any) => {
        if (response && response.token) {
          this.saveToken(response.token);
          // Зберігаємо username і roles
          const userInfo: UserInfo = {
            username: response.username || username,
            roles: response.roles || [role]
          };
          this.userInfo.next(userInfo);
          localStorage.setItem('userInfo', JSON.stringify(userInfo));
          console.log('Токен і дані збережено після реєстрації:', response.token, userInfo);
        } else {
          throw new Error('Токен не отримано від сервера');
        }
      }),
      catchError((err: HttpErrorResponse) => {
        console.error('Помилка реєстрації:', err);
        return throwError(() => new Error(err.error?.message || 'Помилка реєстрації'));
      })
    );
  }

  // Збереження токену
  saveToken(token: string): void {
    if (token && typeof token === 'string') {
      localStorage.setItem('token', token);
    } else {
      console.error('Некоректний токен:', token);
    }
  }

  // Отримання токену
  getToken(): string | null {
    const token = localStorage.getItem('token');
    return token && token !== 'EROR' ? token : null;
  }

  // Вихід із системи
  logout(): void {
    this.userInfo.next(null);
    localStorage.removeItem('token');
    localStorage.removeItem('userInfo');
    console.log('Користувач вийшов із системи');
    this.router.navigate(['/login']);
  }

  // Перевірка сесії
  checkSession(): Observable<boolean> {
    const token = this.getToken();
    if (!token) {
      console.warn('Токен відсутній, сесія невалідна');
      return of(false);
    }

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    return this.http.get<CheckSessionResponse>(`${this.apiUrl}/check-session`, { headers }).pipe(
      tap((response) => {
        // Зберігаємо дані з check-session
        const userInfo: UserInfo = {
          username: response.username,
          roles: response.roles || []
        };
        this.userInfo.next(userInfo);
        localStorage.setItem('userInfo', JSON.stringify(userInfo));
        console.log('Сесія валідна:', userInfo);
      }),
      map(() => true),
      catchError((err: HttpErrorResponse) => {
        console.error('Помилка перевірки сесії:', err.status, err.error);
        this.logout();
        return of(false);
      })
    );
  }

  // Отримання ролей з токена
  private getUserRolesFromToken(token: string): string[] {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const roleClaim = payload['role'] || payload['roles'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      return roleClaim ? (Array.isArray(roleClaim) ? roleClaim : [roleClaim]) : [];
    } catch (err) {
      console.error('Помилка при декодуванні токена:', err);
      return [];
    }
  }

  // Отримання ролей користувача
  getUserRoles(): string[] {
    const userInfo = this.userInfo.value;
    return userInfo ? userInfo.roles : [];
  }

  // Чи має користувач вказану роль
  hasRole(role: string): boolean {
    return this.getUserRoles().includes(role);
  }

  // Чи має хоча б одну з переданих ролей
  hasAnyRole(roles: string[]): boolean {
    const userRoles = this.getUserRoles();
    return roles.some((r) => userRoles.includes(r));
  }
}
