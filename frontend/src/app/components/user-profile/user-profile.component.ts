import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, of } from 'rxjs';
import { finalize, takeUntil, switchMap, tap, catchError } from 'rxjs/operators';

// Твої сервіси
import { ProfileService } from '../../services/profile.service';
import { AuthService } from '../../services/auth.service';
import { FollowService } from '../../services/follow.service';
import { ChatService } from '../../services/chat.service';
import { RatingService } from '../../services/rating.service';

// Твої моделі
import { Profile } from '../../models/profile.model';

// Дочірні компоненти
import { ProfileCardComponent } from './profile-card/profile-card.component';
import { ProfileHeatmapComponent } from './profile-heatmap/profile-heatmap.component';
import { ProfileRadarChartComponent } from './profile-radar-chart/profile-radar-chart.component';
import { ProfileBadgesComponent } from './profile-badges/profile-badges.component';

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
    ProfileBadgesComponent
  ]
})
export class UserProfileComponent implements OnInit, OnDestroy {
  profile?: Profile;
  currentUserId?: string;

  loading = false;
  savingDescription = false;
  uploadingPhoto = false;
  isLoadingFollow = false;
  isFollowing = false;
  error?: string;

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
    // 1. Отримуємо дані про поточного юзера
    this.authService.getUserInfo$()
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => this.currentUserId = user?.id);

    // 2. Слідкуємо за параметрами URL
    this.route.paramMap.pipe(
      tap(() => {
        this.loading = true;
        this.error = undefined;
      }),
      switchMap(params => {
        const userId = params.get('userId');
        return this.profileService.getProfile(userId ?? undefined).pipe(
          catchError(err => {
            this.error = 'Профіль не знайдено';
            return of(undefined);
          })
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe(p => {
      this.profile = p;
      this.loading = false;

      if (p) {
        // 3. Якщо профіль не мій, перевіряємо чи я підписаний
        if (!this.isOwnProfile) {
          this.checkFollowStatus(p.userId);
        }
      }
    });
  }

  // --- Гетери ---

  get isOwnProfile(): boolean {
    if (!this.profile || !this.currentUserId) return false;
    return String(this.profile.userId) === String(this.currentUserId);
  }

  // --- Приватні методи ---

  private checkFollowStatus(targetUserId: string): void {
    this.followService.isFollowing(targetUserId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(status => this.isFollowing = status);
  }

  // --- Обробники подій від <app-profile-card> ---

  onPhotoSelected(e: Event): void {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.uploadingPhoto = true;
    this.profileService.updatePhoto(file)
      .pipe(finalize(() => this.uploadingPhoto = false))
      .subscribe({
        next: () => {
          const currentId = this.route.snapshot.paramMap.get('userId');
          // Оновлюємо профіль після завантаження
          this.profileService.getProfile(currentId ?? undefined).subscribe(p => this.profile = p);
        },
        error: () => this.error = 'Помилка завантаження фото'
      });
  }

  saveDescription(text: string): void {
    this.savingDescription = true;

    this.profileService.updateDescription(text)
      .pipe(finalize(() => this.savingDescription = false))
      .subscribe({
        next: () => {
          if (this.profile) this.profile.description = text;
        },
        error: () => this.error = 'Не вдалося зберегти опис'
      });
  }

  toggleFollow(): void {
    if (!this.profile || this.isLoadingFollow || this.isOwnProfile) return;

    this.isLoadingFollow = true;
    const targetId = this.profile.userId;

    if (this.isFollowing) {
      this.followService.unfollow(targetId)
        .pipe(finalize(() => this.isLoadingFollow = false))
        .subscribe({
          next: () => {
            this.isFollowing = false;
            if (this.profile) this.profile.followersCount--;
          },
          error: () => this.error = 'Не вдалося скасувати підписку'
        });
    } else {
      this.followService.follow(targetId)
        .pipe(finalize(() => this.isLoadingFollow = false))
        .subscribe({
          next: () => {
            this.isFollowing = true;
            if (this.profile) this.profile.followersCount++;
          },
          error: () => this.error = 'Не вдалося підписатися'
        });
    }
  }

  sendMessage(): void {
    const targetUserId = this.profile?.userId;
    if (!targetUserId || !this.currentUserId) return;

    this.loading = true;

    this.chatService.createChat(this.currentUserId, targetUserId)
      .pipe(
        finalize(() => this.loading = false),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => {
          this.router.navigate(['/chat']);
        },
        error: (err) => {
          this.error = 'Не вдалося відкрити діалог';
          console.error(err);
        }
      });
  }

  setRate(value: number): void {
    if (!this.profile || this.isOwnProfile || this.loading) return;

    this.ratingService.rateProfile(this.profile.profileId, value)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          if (this.profile) {
            this.profile.rating = res.averageRating;
          }
        },
        error: (err) => {
          this.error = 'Не вдалося зберегти оцінку';
          console.error(err);
        }
      });
  }

  onLogout(): void {
    if (confirm('Ви впевнені, що хочете вийти?')) {
      this.authService.logout();
      this.router.navigate(['/login']);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
