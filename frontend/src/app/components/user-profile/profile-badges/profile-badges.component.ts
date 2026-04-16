import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { forkJoin } from 'rxjs';
import { BadgeService } from '../../../services/badge.service';
import { Badge, ProfileBadgeViewModel } from '../../../models/profile-badge.model';

export interface BadgeGroup {
  typeName: string;
  badges: ProfileBadgeViewModel[];
}

@Component({
  selector: 'app-profile-badges',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-badges.component.html',
  styleUrls: ['./profile-badges.component.scss']
})
export class ProfileBadgesComponent implements OnChanges {
  @Input({ required: true }) profileId!: string;
  @Input() isOwnProfile = false;

  badges: ProfileBadgeViewModel[] = [];
  unlockedBadges: ProfileBadgeViewModel[] = [];
  groupedBadges: BadgeGroup[] = [];

  loading = false;
  error = '';
  isModalOpen = false;

  constructor(public badgeService: BadgeService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['profileId']?.currentValue) {
      this.loadBadges();
    }
  }

  private loadBadges(): void {
    if (!this.profileId?.trim()) return;

    this.loading = true;
    this.error = '';

    forkJoin({
      allBadges: this.badgeService.getAll(),
      profileBadges: this.badgeService.getByProfileId(this.profileId)
    }).subscribe({
      next: ({ allBadges, profileBadges }) => {
        const ownedIds = new Set((profileBadges ?? []).map(x => x.id));

        this.badges = (allBadges ?? []).map(badge => ({
          ...badge,
          unlocked: ownedIds.has(badge.id)
        }));

        this.unlockedBadges = this.badges.filter(b => b.unlocked);
        this.groupBadges();
        this.loading = false;
      },
      error: (err) => {
        console.error('Badge loading error', err);
        this.error = 'Не вдалося завантажити досягнення.';
        this.loading = false;
      }
    });
  }

  private groupBadges(): void {
    const map = new Map<string, ProfileBadgeViewModel[]>();
    this.badges.forEach(badge => {
      const typeName = badge.badgeType?.name || 'Інші нагороди';
      if (!map.has(typeName)) map.set(typeName, []);
      map.get(typeName)!.push(badge);
    });

    this.groupedBadges = Array.from(map.entries()).map(([typeName, badges]) => ({
      typeName,
      badges
    }));
  }

  removeBadge(badge: ProfileBadgeViewModel, event: MouseEvent): void {
    event.stopPropagation();
    if (!confirm(`Прибрати нагороду «${badge.name}» з профілю?`)) return;

    this.badgeService.removeFromProfile(this.profileId, badge.id).subscribe({
      next: () => {
        this.badges = this.badges.map(b =>
          b.id === badge.id ? { ...b, unlocked: false } : b
        );
        this.unlockedBadges = this.badges.filter(b => b.unlocked);
        this.groupBadges();
      },
      error: (err) => {
        console.error('Помилка при видаленні нагороди', err);
        this.error = 'Не вдалося прибрати нагороду.';
      }
    });
  }

  getCleanImageSrc(base64: string | null | undefined): string {
    if (!base64) return this.badgeService.getBadgeImageSrc(null);
    return this.badgeService.getBadgeImageSrc(base64.replace(/\s+/g, ''));
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = this.badgeService.getBadgeImageSrc(null);
  }

  openModal(): void {
    this.isModalOpen = true;
    document.body.style.overflow = 'hidden';
  }

  closeModal(): void {
    this.isModalOpen = false;
    document.body.style.overflow = '';
  }

  onBackdropClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-backdrop')) {
      this.closeModal();
    }
  }

  get unlockedCount(): number {
    return this.unlockedBadges.length;
  }

  get progressPercentage(): number {
    if (this.badges.length === 0) return 0;
    return Math.round((this.unlockedCount / this.badges.length) * 100);
  }
}

