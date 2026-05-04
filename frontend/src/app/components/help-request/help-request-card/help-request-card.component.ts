import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { v4 as uuidv4 } from 'uuid';

import { HelpRequest, HelpRequestItem } from '../../../models/help-request.model';
import { ProfileService } from '../../../services/profile.service';
import { AuthService } from '../../../services/auth.service';
import { RelativeTimePipe } from '../../../pipes/relative-time.pipe';
import { PostComment, CreatePostCommentDto } from '../../../models/post-comment.model';
import { PostLikeService } from '../../../services/post-like.service';
import { PostCommentService } from '../../../services/post-comment.service';
import { ShareToChatModalComponent } from '../../../dialog/share-to-chat-modal/share-to-chat-modal.component';
import { DonateModalComponent } from '../../../dialog/donate-modal/donate-modal.component';
import { RoleBadgeComponent } from '../../../shared/role-badge/role-badge.component';

@Component({
  selector: 'app-help-request-card',
  standalone: true,
  imports: [CommonModule, FormsModule, RelativeTimePipe, ShareToChatModalComponent, DonateModalComponent, RoleBadgeComponent],
  templateUrl: './help-request-card.component.html',
  styleUrls: ['./help-request-card.component.scss']
})
export class HelpRequestCardComponent implements OnInit {
  public profileService = inject(ProfileService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private likeService = inject(PostLikeService);
  private commentService = inject(PostCommentService);

  @Input({ required: true }) request!: HelpRequest;
  @Output() profileClick = new EventEmitter<HelpRequest>();

  isLiked = false;
  likesCount = 0;
  commentsCount = 0;
  isLikePending = false;

  showComments = false;
  comments: PostComment[] = [];
  newCommentText = '';
  isLoadingComments = false;
  isSendingComment = false;
  currentUserId = '';

  isShareModalOpen = false;
  isDonateModalOpen = false;

  ngOnInit(): void {
    this.likesCount = Number(this.request.likesCount) || 0;
    this.commentsCount = Number(this.request.commentsCount) || 0;
    this.isLiked = !!this.request.isLikedByCurrentUser;

    this.authService.getUserInfo$()
      .pipe(filter(user => !!user))
      .subscribe(user => {
        this.currentUserId = user!.id;
      });
  }

  get authorName(): string {
    return this.request.authorUserName
      || this.request.userName
      || this.request.authorFullName
      || this.request.authorName
      || 'Unknown';
  }

  get authorAvatarBase64(): string | null {
    const raw = this.request.authorPhotoBase64
      || this.request.userPhotoBase64
      || this.request.profilePhotoBase64
      || this.request.photoBase64
      || this.request.photo
      || null;

    return typeof raw === 'string' && raw.trim() ? raw : null;
  }

  get requestImageSrc(): string | null {
    const image = this.request.image || this.request.imageBase64 || null;
    if (!image?.trim()) return null;
    return image.startsWith('data:image/') ? image : `data:image/jpeg;base64,${image}`;
  }

  get locationLabel(): string | null {
    return this.request.locationName
      || this.request.locationAddress
      || this.request.districtName
      || this.request.address
      || null;
  }

  get totalRequestedSum(): number {
    const explicitTarget = Number(this.request.targetAmount) || 0;
    if (explicitTarget > 0) return explicitTarget;

    return this.request.requestItems?.reduce((sum, item) => sum + this.getItemTotal(item), 0) ?? 0;
  }

  get collectedAmount(): number {
    const collected = Number(this.request.collectedAmount) || 0;
    const payments = Number(this.request.totalPayments) || 0;
    return Math.max(collected, payments, 0);
  }

  get remainingAmount(): number {
    return Math.max(this.totalRequestedSum - this.collectedAmount, 0);
  }

  get progressPercent(): number {
    if (this.totalRequestedSum <= 0) return 0;
    return Math.max(0, Math.min(100, Math.round((this.collectedAmount / this.totalRequestedSum) * 100)));
  }

  get isCompleted(): boolean {
    return this.progressPercent >= 100;
  }

  get isActive(): boolean {
    return this.request.isActive !== false && !this.isCompleted;
  }

  get statusLabel(): string {
    if (this.request.isActive === false) return 'Збір закрито';
    if (this.isCompleted) return 'Зібрано';
    return 'Активний збір';
  }

  getProfileRouteValue(): string | null {
    return this.request.authorUserName?.trim()
      || this.request.userName?.trim()
      || this.request.userId?.trim()
      || null;
  }

  openRequestAuthorProfile(): void {
    const routeValue = this.getProfileRouteValue();
    if (!routeValue) return;
    this.router.navigate(['/profile', routeValue]);
  }

  openProfile(usernameOrId?: string | null): void {
    if (!usernameOrId?.trim()) return;
    this.router.navigate(['/profile', usernameOrId.trim()]);
  }

  getInitials(name: string | undefined | null): string {
    if (!name?.trim()) return '?';
    const parts = name.trim().split(/\s+/).slice(0, 2);
    return parts.map(p => p[0]?.toUpperCase() || '').join('') || '?';
  }

  getItemTotal(item: HelpRequestItem): number {
    return (Number(item.quantity) || 0) * (Number(item.unitPrice) || 0);
  }

  toggleLike(): void {
    if (!this.currentUserId || this.isLikePending) return;

    const dto = { postId: this.request.id, userId: this.currentUserId };
    const previousLiked = this.isLiked;
    const previousCount = this.likesCount;

    this.isLikePending = true;
    this.isLiked = !previousLiked;
    this.likesCount = Math.max(0, previousCount + (this.isLiked ? 1 : -1));
    this.request.isLikedByCurrentUser = this.isLiked;
    this.request.likesCount = this.likesCount;

    const request$ = this.isLiked
      ? this.likeService.likePost(dto)
      : this.likeService.unlikePost(dto);

    request$.subscribe({
      next: () => {
        this.isLikePending = false;
      },
      error: () => {
        this.isLiked = previousLiked;
        this.likesCount = previousCount;
        this.request.isLikedByCurrentUser = previousLiked;
        this.request.likesCount = previousCount;
        this.isLikePending = false;
      }
    });
  }

  toggleComments(): void {
    this.showComments = !this.showComments;
    if (this.showComments && this.comments.length === 0) {
      this.loadComments();
    }
  }

  loadComments(): void {
    this.isLoadingComments = true;

    this.commentService.getCommentsByPost(this.request.id).subscribe({
      next: data => {
        this.comments = (data ?? []).map(c => ({
          ...c,
          userPhotoBase64: c.userPhotoBase64 || (c as any).authorPhotoBase64 || (c as any).profilePhotoBase64 || ''
        }));
        this.commentsCount = this.comments.length;
        this.request.commentsCount = this.commentsCount;
        this.isLoadingComments = false;
      },
      error: () => {
        this.isLoadingComments = false;
      }
    });
  }

  sendComment(): void {
    const text = this.newCommentText.trim();
    if (!text || !this.currentUserId || this.isSendingComment) return;

    const dto: CreatePostCommentDto = {
      postId: this.request.id,
      text,
      requestId: uuidv4()
    };

    this.isSendingComment = true;
    this.commentService.addComment(dto).subscribe({
      next: () => {
        this.newCommentText = '';
        this.isSendingComment = false;
        this.loadComments();
      },
      error: err => {
        console.error('Помилка додавання коментаря', err);
        this.isSendingComment = false;
      }
    });
  }

  openShareModal(): void {
    this.isShareModalOpen = true;
  }

  closeShareModal(): void {
    this.isShareModalOpen = false;
  }

  onShared(): void {
    alert('Реквест надіслано в чат');
  }

  openDonateModal(): void {
    this.isDonateModalOpen = true;
  }

  closeDonateModal(): void {
    this.isDonateModalOpen = false;
  }

  trackByRequestItem(_: number, item: HelpRequestItem): string {
    return item.id;
  }

  trackByComment(_: number, comment: PostComment): string {
    return comment.id;
  }
}
