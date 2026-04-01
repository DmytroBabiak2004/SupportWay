import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, of, Observable } from 'rxjs';

import { finalize, takeUntil, switchMap, tap, catchError } from 'rxjs/operators';

import { ProfileService } from '../../services/profile.service';
import { AuthService } from '../../services/auth.service';
import { FollowService } from '../../services/follow.service';
import { ChatService } from '../../services/chat.service';
import { RatingService } from '../../services/rating.service';
import { Profile } from '../../models/profile.model';

import { ProfileCardComponent } from './profile-card/profile-card.component';
import { ProfileHeatmapComponent } from './profile-heatmap/profile-heatmap.component';
import { ProfileRadarChartComponent } from './profile-radar-chart/profile-radar-chart.component';
import { ProfileBadgesComponent } from './profile-badges/profile-badges.component';
import { FollowModalComponent } from './follow-modal/follow-modal.component';
import { VerificationModalComponent } from './verification-modal/verification-modal.component';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ProfileCardComponent,
    ProfileHeatmapComponent,
    ProfileRadarChartComponent,
    ProfileBadgesComponent,
    FollowModalComponent,
    VerificationModalComponent,
  ]
})
export class UserProfileComponent implements OnInit, OnDestroy {
  profile?: Profile;
  currentUserId?: string;

  loading           = false;
  savingDescription = false;
  uploadingPhoto    = false;
  isLoadingFollow   = false;
  isFollowing       = false;
  error?: string;

  showFollowModal       = false;
  followModalMode: 'followers' | 'following' = 'followers';
  showVerificationModal = false;

  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    protected profileService: ProfileService,
    private followService: FollowService,
    private chatService: ChatService,
    private ratingService: RatingService
  ) {}

  ngOnInit(): void {
    this.authService.getUserInfo$()
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => this.currentUserId = user?.id);

    this.route.paramMap.pipe(
      tap(() => { this.loading = true; this.error = undefined; }),
      switchMap(params => {
        const userId = params.get('userId');
        return this.profileService.getProfile(userId ?? undefined).pipe(
          catchError(() => { this.error = 'Профіль не знайдено'; return of(undefined); })
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe(p => {
      this.profile = p;
      this.loading = false;
      if (p && !this.isOwnProfile) this.checkFollowStatus(p.userId);
    });
  }

  get isOwnProfile(): boolean {
    return !!this.profile && !!this.currentUserId &&
      String(this.profile.userId) === String(this.currentUserId);
  }

  private checkFollowStatus(targetUserId: string): void {
    this.followService.isFollowing(targetUserId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(s => this.isFollowing = s);
  }

  onPhotoSelected(e: Event): void {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.uploadingPhoto = true;
    this.profileService.updatePhoto(file)
      .pipe(finalize(() => this.uploadingPhoto = false))
      .subscribe({
        next: () => this.reloadProfile(),
        error: () => this.error = 'Помилка завантаження фото'
      });
  }

  saveDescription(text: string): void {
    this.savingDescription = true;
    this.profileService.updateDescription(text)
      .pipe(finalize(() => this.savingDescription = false))
      .subscribe({
        next: () => { if (this.profile) this.profile.description = text; },
        error: () => this.error = 'Не вдалося зберегти опис'
      });
  }

  toggleFollow(): void {
    if (!this.profile || this.isLoadingFollow || this.isOwnProfile) return;
    this.isLoadingFollow = true;

    const targetId = this.profile.userId;

    // ЯВНО вказуємо тип Observable<unknown>, щоб уникнути Union Type помилок
    const action$: Observable<unknown> = this.isFollowing
      ? this.followService.unfollow(targetId)
      : this.followService.follow(targetId);

    action$.pipe(
      finalize(() => this.isLoadingFollow = false),
      takeUntil(this.destroy$)
    ).subscribe({
      next: () => {
        const delta = this.isFollowing ? -1 : 1;
        this.isFollowing = !this.isFollowing;
        if (this.profile) this.profile.followersCount += delta;
      },
      error: () => { // Прибрали (err: any), якщо помилка ніде не використовується
        this.error = 'Не вдалося виконати дію';
      }
    });
    // Жодних `as any` більше не потрібно!
  }
  sendMessage(): void {
    const targetId = this.profile?.userId;
    if (!targetId || !this.currentUserId) return;
    this.loading = true;
    this.chatService.createOrGetChat(this.currentUserId, targetId)
      .pipe(finalize(() => this.loading = false), takeUntil(this.destroy$))
      .subscribe({
        next: chat => this.router.navigate(['/chat'], { queryParams: { chatId: chat.id } }),
        error: () => this.error = 'Не вдалося відкрити діалог'
      });
  }

  setRate(value: number): void {
    if (!this.profile || this.isOwnProfile) return;
    this.ratingService.rateProfile(this.profile.profileId, value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: res => { if (this.profile) this.profile.rating = res.averageRating; },
        error: () => this.error = 'Не вдалося зберегти оцінку'
      });
  }

  onLogout(): void {
    if (confirm('Вийти з акаунту?')) {
      this.authService.logout();
      this.router.navigate(['/login']);
    }
  }

  openFollowers(): void { this.followModalMode = 'followers'; this.showFollowModal = true; }
  openFollowing(): void { this.followModalMode = 'following'; this.showFollowModal = true; }

  onFollowCountChanged(): void { this.reloadProfile(); }

  openVerificationModal(): void { this.showVerificationModal = true; }

  reloadProfile(): void {
    const id = this.route.snapshot.paramMap.get('userId');
    this.profileService.getProfile(id ?? undefined)
      .subscribe(p => { if (p) this.profile = p; });
  }

  ngOnDestroy(): void { this.destroy$.next(); this.destroy$.complete(); }
}
