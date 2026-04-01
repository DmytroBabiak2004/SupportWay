import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FollowService } from '../../../services/follow.service';
import { AuthService } from '../../../services/auth.service';
import { ProfileService } from '../../../services/profile.service';
import { FollowUser } from '../../../models/follow-user.model';
import { VERIFICATION_ICONS, VERIFICATION_COLORS } from '../../../models/verification.model';

@Component({
  selector: 'app-follow-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './follow-modal.component.html',
  styleUrls: ['./follow-modal.component.scss']
})
export class FollowModalComponent implements OnInit {
  @Input({ required: true }) userId!: string;
  @Input({ required: true }) mode!: 'followers' | 'following';
  @Input() isOwnProfile = false;
  @Output() closed = new EventEmitter<void>();
  @Output() countChanged = new EventEmitter<void>();

  users: FollowUser[] = [];
  filtered: FollowUser[] = [];
  searchText = '';
  loading = true;
  currentUserId = '';

  readonly ICONS   = VERIFICATION_ICONS;
  readonly COLORS  = VERIFICATION_COLORS;

  constructor(
    private followService: FollowService,
    private authService: AuthService,
    private router: Router,
    public profileService: ProfileService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getUserId();
    this.load();
  }

  load(): void {
    this.loading = true;
    const req$ = this.mode === 'followers'
      ? this.followService.getFollowers(this.userId)
      : this.followService.getFollowing(this.userId);

    req$.subscribe({
      next: list => {
        this.users   = list;
        this.filter();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  filter(): void {
    const t = this.searchText.trim().toLowerCase();
    this.filtered = t
      ? this.users.filter(u =>
          (u.username ?? '').toLowerCase().includes(t) ||
          (u.name     ?? '').toLowerCase().includes(t) ||
          (u.fullName ?? '').toLowerCase().includes(t))
      : [...this.users];
  }

  get title(): string {
    return this.mode === 'followers' ? 'Підписники' : 'Підписки';
  }

  openProfile(u: FollowUser): void {
    this.closed.emit();
    this.router.navigate(['/profile', u.userId]);
  }

  removeFollower(u: FollowUser): void {
    if (!confirm(`Видалити @${u.username} з підписників?`)) return;
    this.followService.removeFollower(u.userId).subscribe({
      next: () => {
        this.users    = this.users.filter(x => x.userId !== u.userId);
        this.filter();
        this.countChanged.emit();
      }
    });
  }

  unfollow(u: FollowUser): void {
    if (!confirm(`Відписатися від @${u.username}?`)) return;
    this.followService.unfollow(u.userId).subscribe({
      next: () => {
        this.users    = this.users.filter(x => x.userId !== u.userId);
        this.filter();
        this.countChanged.emit();
      }
    });
  }

  getDisplayName(u: FollowUser): string {
    if (u.name && u.fullName) return `${u.name} ${u.fullName}`;
    return u.name || u.fullName || u.username;
  }

  getInitials(u: FollowUser): string {
    const name = this.getDisplayName(u);
    return name.split(' ').slice(0,2).map(n => n[0]?.toUpperCase()).join('');
  }
}
