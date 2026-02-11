import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterModule, Router } from '@angular/router';
import { AsyncPipe, NgClass, NgIf } from '@angular/common';
import { Subscription, of } from 'rxjs';
import { switchMap, tap, catchError } from 'rxjs/operators';

import { AuthService, UserInfo } from '../../services/auth.service';
import { ProfileService } from '../../services/profile.service';
import { Profile } from '../../models/profile.model';
import { UserSearchComponent } from './user-search/user-search.component';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  standalone: true,
  imports: [RouterModule, NgClass, UserSearchComponent, NgIf, AsyncPipe],
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {
  isMenuOpen = false;
  isDropdownOpen = false;
  isUserDropdownOpen = false;

  currentUser: UserInfo | null = null;
  userProfile: Profile | null = null;

  private subscription = new Subscription();

  constructor(
    public authService: AuthService,
    public profileService: ProfileService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const authSub = this.authService.getUserInfo$().pipe(
      tap(user => {
        this.currentUser = user;
        if (user && !this.userProfile) {
          this.userProfile = { username: user.username } as Profile;
        }
      }),
      switchMap(user => {
        if (!user) return of(null);
        // Отримуємо свій профіль (метод без параметрів у вашому сервісі йде на /me)
        return this.profileService.getProfile().pipe(
          catchError(err => {
            console.error('Помилка завантаження профілю:', err);
            return of(null);
          })
        );
      })
    ).subscribe(profile => {
      if (profile) {
        this.userProfile = profile;
      }
    });

    this.subscription.add(authSub);
  }

  // --- МЕТОД ДЛЯ ПЕРЕХОДУ В ПРОФІЛЬ ---
  goToMyProfile(): void {
    const username = this.userProfile?.username || this.currentUser?.username;
    if (username) {
      this.handleNavClick();
      this.router.navigate(['/profile', username]);
    }
  }

  get initials(): string {
    const p = this.userProfile;
    if (p?.name || p?.fullName) {
      const f = p.name ? p.name[0] : '';
      const l = p.fullName ? p.fullName[0] : '';
      return (f + l).toUpperCase();
    }
    return (p?.username?.[0] || this.currentUser?.username?.[0] || '?').toUpperCase();
  }

  get displayName(): string {
    return this.userProfile?.username || this.currentUser?.username || 'Гість';
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
    if (!this.isMenuOpen) {
      this.isDropdownOpen = false;
    }
  }

  toggleDropdown(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  handleNavClick(): void {
    this.isMenuOpen = false;
    this.isDropdownOpen = false;
    this.isUserDropdownOpen = false;
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
