import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, of } from 'rxjs';
import { finalize, takeUntil, switchMap, tap, catchError } from 'rxjs/operators';

import { ProfileService } from '../../services/profile.service';
import { AuthService } from '../../services/auth.service';
import { FollowService } from '../../services/follow.service';
import { Profile } from '../../models/profile.model';
import { ChatService } from '../../services/chat.service';
import {RatingService} from '../../services/rating.service';

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe]
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
  isEditingDescription = false;
  descriptionDraft = '';

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
        this.descriptionDraft = p.description ?? '';

        // 3. Якщо профіль не мій, перевіряємо чи я підписаний
        if (!this.isOwnProfile) {
          this.checkFollowStatus(p.userId);
        }
      }
    });
  }

  // Перевірка статусу підписки
  private checkFollowStatus(targetUserId: string): void {
    this.followService.isFollowing(targetUserId)
      .pipe(takeUntil(this.destroy$))
      .subscribe(status => this.isFollowing = status);
  }

  get isOwnProfile(): boolean {
    if (!this.profile || !this.currentUserId) return false;
    return String(this.profile.userId) === String(this.currentUserId);
  }

  get initials(): string {
    if (!this.profile) return '?';
    const first = this.profile.name?.trim()[0] || '';
    const last = this.profile.fullName?.trim()[0] || '';
    if (first || last) return (first + last).toUpperCase();
    return (this.profile.username?.[0] || '?').toUpperCase();
  }

  // --- Actions ---

  toggleFollow(): void {
    if (!this.profile || this.isLoadingFollow || this.isOwnProfile) return;

    this.isLoadingFollow = true;
    const targetId = this.profile.userId;

    if (this.isFollowing) {
      // Відписка
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
      // Підписка
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

    // Створюємо або отримуємо існуючий чат
    this.chatService.createChat(this.currentUserId, targetUserId)
      .pipe(
        finalize(() => this.loading = false),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (chat) => {
          this.router.navigate(['/chat']);
        },
        error: (err) => {
          this.error = 'Не вдалося відкрити діалог';
          console.error(err);
        }
      });
  }

  startEditDescription(): void {
    this.descriptionDraft = this.profile?.description ?? '';
    this.isEditingDescription = true;
  }

  cancelEditDescription(): void {
    this.isEditingDescription = false;
  }

  saveDescription(): void {
    const text = this.descriptionDraft.trim();
    this.savingDescription = true;

    this.profileService.updateDescription(text)
      .pipe(finalize(() => this.savingDescription = false))
      .subscribe({
        next: () => {
          if (this.profile) this.profile.description = text;
          this.isEditingDescription = false;
        },
        error: () => this.error = 'Не вдалося зберегти опис'
      });
  }

  onPhotoSelected(e: Event): void {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.uploadingPhoto = true;
    this.profileService.updatePhoto(file)
      .pipe(finalize(() => this.uploadingPhoto = false))
      .subscribe({
        next: () => {
          const currentId = this.route.snapshot.paramMap.get('userId');
          this.profileService.getProfile(currentId ?? undefined).subscribe(p => this.profile = p);
        },
        error: () => this.error = 'Помилка завантаження фото'
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
