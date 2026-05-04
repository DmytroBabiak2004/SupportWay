import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, inject, OnChanges, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { v4 as uuidv4 } from 'uuid';

import { ChatService, SharedPreview } from '../../services/chat.service';
import { ProfileService } from '../../services/profile.service';
import { PostLikeService } from '../../services/post-like.service';
import { PostCommentService } from '../../services/post-comment.service';
import { AuthService } from '../../services/auth.service';
import { PostComment, CreatePostCommentDto } from '../../models/post-comment.model';
import { RoleBadgeComponent } from '../../shared/role-badge/role-badge.component';

@Component({
  selector: 'app-shared-post-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, RoleBadgeComponent],
  templateUrl: './shared-post-dialog.component.html',
  styleUrls: ['./shared-post-dialog.component.scss']
})
export class SharedPostDialogComponent implements OnChanges {
  public profileService = inject(ProfileService);

  private chatService = inject(ChatService);
  private postLikeService = inject(PostLikeService);
  private postCommentService = inject(PostCommentService);
  private authService = inject(AuthService);
  private router = inject(Router);

  @Input() isOpen = false;
  @Input() preview: SharedPreview | null | undefined = null;
  @Input() sharedPostId: string | null | undefined = null;
  @Input() sharedHelpRequestId: string | null | undefined = null;

  @Output() close = new EventEmitter<void>();

  currentUserId = '';

  isLoading = false;

  isLiked = false;
  likesCount = 0;
  isLikePending = false;

  showComments = true;
  comments: PostComment[] = [];
  commentsCount = 0;
  newCommentText = '';
  isLoadingComments = false;
  isSendingComment = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen'] && this.isOpen) {
      this.initCurrentUser();
      this.loadDetails();
    }
  }

  private initCurrentUser(): void {
    this.authService.getUserInfo$()
      .pipe(filter(user => !!user))
      .subscribe(user => {
        this.currentUserId = user!.id;
      });
  }

  private loadDetails(): void {
    const entityId = this.getSharedEntityId();

    this.likesCount = 0;
    this.commentsCount = 0;
    this.comments = [];
    this.newCommentText = '';
    this.isLiked = false;

    if (!entityId) {
      return;
    }

    this.isLoading = true;
    this.isLoadingComments = true;

    this.postLikeService.getLikesCount(entityId).subscribe({
      next: count => {
        this.likesCount = count ?? 0;
      },
      error: () => {
        this.likesCount = 0;
      }
    });

    /*
      Якщо в тебе в PostLikeService вже є метод перевірки лайку поточного користувача,
      наприклад isLikedByCurrentUser(postId), можеш тут його підключити.
      Поки що стан лайку в модалці буде оновлюватись після кліку оптимістично.
    */

    this.postCommentService.getCommentsByPost(entityId).subscribe({
      next: comments => {
        this.comments = (comments ?? []).map(c => ({
          ...c,
          userPhotoBase64: c.userPhotoBase64 || (c as any).authorPhotoBase64 || (c as any).profilePhotoBase64 || ''
        }));

        this.commentsCount = this.comments.length;
        this.isLoading = false;
        this.isLoadingComments = false;
      },
      error: () => {
        this.comments = [];
        this.commentsCount = 0;
        this.isLoading = false;
        this.isLoadingComments = false;
      }
    });
  }

  closeDialog(event?: MouseEvent): void {
    event?.stopPropagation();
    this.close.emit();
  }

  stop(event: MouseEvent): void {
    event.stopPropagation();
  }

  isHelpRequest(): boolean {
    return this.preview?.entityType === 'helpRequest' || !!this.sharedHelpRequestId;
  }

  isPost(): boolean {
    return !this.isHelpRequest();
  }

  getSharedEntityId(): string {
    return this.sharedPostId || this.sharedHelpRequestId || this.preview?.id || '';
  }

  getEntityLabel(): string {
    return this.isHelpRequest() ? 'Запит допомоги' : 'Пост';
  }

  getAuthorName(): string {
    return this.preview?.authorUserName?.trim() || 'Користувач';
  }

  getAuthorAvatar(): string | null {
    return this.getImageSrc(
      this.preview?.authorPhotoBase64 ||
      (this.preview as any)?.userPhotoBase64 ||
      (this.preview as any)?.profilePhotoBase64 ||
      (this.preview as any)?.photoBase64 ||
      (this.preview as any)?.photo ||
      (this.preview as any)?.avatarUrl ||
      (this.preview as any)?.avatar
    );
  }

  getInitials(name?: string | null): string {
    if (!name?.trim()) return '?';

    return name
      .trim()
      .split(/\s+/)
      .slice(0, 2)
      .map(part => part.charAt(0).toUpperCase())
      .join('');
  }

  getPreviewImage(): string | null {
    return this.chatService.getPreviewImageSrc(this.preview?.imageBase64);
  }

  getContent(): string {
    return this.preview?.content?.trim() || '';
  }

  getTitle(): string {
    return this.preview?.title?.trim() || '';
  }

  openAuthorProfile(): void {
    const username = this.preview?.authorUserName?.trim();
    if (!username) return;

    this.close.emit();
    this.router.navigate(['/profile', username]);
  }

  openProfile(usernameOrId?: string | null): void {
    if (!usernameOrId?.trim()) return;

    this.close.emit();
    this.router.navigate(['/profile', usernameOrId.trim()]);
  }

  toggleLike(): void {
    const entityId = this.getSharedEntityId();
    if (!entityId || !this.currentUserId || this.isLikePending) {
      return;
    }

    const previousLiked = this.isLiked;
    const previousCount = this.likesCount;

    this.isLikePending = true;
    this.isLiked = !previousLiked;
    this.likesCount = Math.max(0, previousCount + (this.isLiked ? 1 : -1));

    const dto = {
      postId: entityId,
      userId: this.currentUserId
    };

    const request$ = this.isLiked
      ? this.postLikeService.likePost(dto)
      : this.postLikeService.unlikePost(dto);

    request$.subscribe({
      next: () => {
        this.isLikePending = false;
      },
      error: () => {
        this.isLiked = previousLiked;
        this.likesCount = previousCount;
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
    const entityId = this.getSharedEntityId();
    if (!entityId) return;

    this.isLoadingComments = true;

    this.postCommentService.getCommentsByPost(entityId).subscribe({
      next: comments => {
        this.comments = (comments ?? []).map(c => ({
          ...c,
          userPhotoBase64: c.userPhotoBase64 || (c as any).authorPhotoBase64 || (c as any).profilePhotoBase64 || ''
        }));

        this.commentsCount = this.comments.length;
        this.isLoadingComments = false;
      },
      error: () => {
        this.isLoadingComments = false;
      }
    });
  }

  sendComment(): void {
    const entityId = this.getSharedEntityId();
    const text = this.newCommentText.trim();

    if (!entityId || !text || !this.currentUserId || this.isSendingComment) {
      return;
    }

    const dto: CreatePostCommentDto = {
      postId: entityId,
      text,
      requestId: uuidv4()
    };

    this.isSendingComment = true;

    this.postCommentService.addComment(dto).subscribe({
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

  getCommentAvatar(comment: PostComment): string | null {
    return this.getImageSrc(
      comment.userPhotoBase64 ||
      (comment as any).authorPhotoBase64 ||
      (comment as any).profilePhotoBase64 ||
      (comment as any).photoBase64 ||
      (comment as any).photo ||
      (comment as any).avatarUrl ||
      (comment as any).avatar
    );
  }

  isAuthorVerified(): boolean {
    return !!((this.preview as any)?.authorIsVerified || (this.preview as any)?.isVerified);
  }

  getAuthorVerifiedAs(): number | null {
    const value = (this.preview as any)?.authorVerifiedAs ?? (this.preview as any)?.verifiedAs;
    return typeof value === 'number' ? value : value ? Number(value) : null;
  }

  private getImageSrc(raw?: string | null): string | null {
    if (!raw || typeof raw !== 'string') return null;
    const value = raw.trim();
    if (!value) return null;
    if (value.startsWith('data:image/') || value.startsWith('http://') || value.startsWith('https://') || value.startsWith('blob:')) {
      return value;
    }
    return `data:image/jpeg;base64,${value}`;
  }

  formatDate(value?: string | Date | null): string {
    if (!value) return '';

    return new Intl.DateTimeFormat('uk-UA', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(value));
  }

  formatTime(value?: string | Date | null): string {
    if (!value) return '';

    return new Intl.DateTimeFormat('uk-UA', {
      hour: '2-digit',
      minute: '2-digit'
    }).format(new Date(value));
  }

  trackByComment(_: number, comment: PostComment): string {
    return comment.id;
  }
}
