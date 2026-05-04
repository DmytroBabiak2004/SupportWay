import { Component, OnDestroy, OnInit } from '@angular/core';
import { NgIf } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Subscription, of } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

import { AuthService, UserInfo } from '../../services/auth.service';
import { ProfileService } from '../../services/profile.service';
import { Profile } from '../../models/profile.model';
import { UserSearchComponent } from './user-search/user-search.component';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  standalone: true,
  imports: [RouterModule, UserSearchComponent, NgIf],
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit, OnDestroy {
  isMenuOpen = false;
  isDropdownOpen = false;
  isUserDropdownOpen = false;

  currentUser: UserInfo | null = null;
  isAdmin = false;
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
        this.isAdmin = this.hasAdminRole(user);

        if (user && !this.userProfile) {
          this.userProfile = {
            username: user.username
          } as Profile;
        }

        if (!user) {
          this.userProfile = null;
        }
      }),
      switchMap(user => {
        if (!user) {
          return of(null);
        }

        return this.profileService.getProfile().pipe(
          catchError(error => {
            console.error('Помилка завантаження профілю:', error);
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

  goToMyProfile(): void {
    const username = this.userProfile?.username || this.currentUser?.username;

    if (!username) {
      return;
    }

    this.handleNavClick();
    this.router.navigate(['/profile', username]);
  }

  get initials(): string {
    const profile = this.userProfile;

    if (profile?.name || profile?.fullName) {
      const first = profile.name?.charAt(0) ?? '';
      const second = profile.fullName?.charAt(0) ?? '';
      return `${first}${second}`.toUpperCase() || '?';
    }

    return (
      profile?.username?.charAt(0) ||
      this.currentUser?.username?.charAt(0) ||
      '?'
    ).toUpperCase();
  }

  get displayName(): string {
    return this.userProfile?.username || this.currentUser?.username || 'Гість';
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;

    if (!this.isMenuOpen) {
      this.isDropdownOpen = false;
      this.isUserDropdownOpen = false;
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

  private hasAdminRole(user: UserInfo | null): boolean {
    const rawUser = user as any;

    if (!rawUser) {
      return false;
    }

    if (Array.isArray(rawUser.roles)) {
      return rawUser.roles.some((role: string) => role?.toLowerCase() === 'admin');
    }

    if (typeof rawUser.role === 'string') {
      return rawUser.role.toLowerCase() === 'admin';
    }

    return false;
  }
}
