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

@Component({
  selector: 'app-help-request-card',
  standalone: true,
  imports: [CommonModule, FormsModule, RelativeTimePipe],
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

  showComments = false;
  comments: PostComment[] = [];
  newCommentText = '';
  isLoadingComments = false;
  currentUserId = '';

  ngOnInit(): void {
    // Надійна перевірка на null/undefined
    this.likesCount = this.request.likesCount || 0;
    this.commentsCount = this.request.commentsCount || 0;
    this.isLiked = !!this.request.isLikedByCurrentUser; // Жорстке приведення до boolean

    this.authService.getUserInfo$()
      .pipe(filter(user => !!user))
      .subscribe(user => {
        this.currentUserId = user!.id;

        // Якщо бекенд не повернув статус лайка при завантаженні стрічки,
        // потрібно смикнути API щоб дізнатись, чи лайкав цей юзер пост.
        if (this.request.isLikedByCurrentUser === undefined || this.request.isLikedByCurrentUser === null) {
          this.checkIfUserLiked();
        }
      });
  }

  // Оновлений метод checkIfUserLiked (дублікат видалено)
  private checkIfUserLiked(): void {
    if (!this.currentUserId) return;

    // Оновлюємо кількість лайків
    this.likeService.getLikesCount(this.request.id).subscribe({
      next: count => this.likesCount = count ?? this.likesCount,
      error: () => {}
    });

    // ТУТ ПОТРІБЕН ВИКЛИК: Перевіряємо чи лайкав саме цей юзер.
    // Якщо в likeService немає методу hasUserLikedPost, його треба додати на бекенді та у сервісі.
    // this.likeService.hasUserLikedPost(this.request.id, this.currentUserId).subscribe(isLiked => {
    //    this.isLiked = isLiked;
    // });
  }

  // Виправлення багу з фото (дублікат видалено)
  getRequestImageSrc(image?: string | null): string | null {
    if (!image?.trim()) return null;
    // Якщо база64 не має префіксу, додаємо його
    if (image.startsWith('data:image/')) return image;
    return `data:image/jpeg;base64,${image}`;
  }

  getInitials(name: string | undefined | null): string {
    if (!name?.trim()) return '?';

    const parts = name.trim().split(/\s+/).slice(0, 2);
    return parts.map(p => p[0]?.toUpperCase() || '').join('') || '?';
  }

  getProfileRouteValue(): string | null {
    return this.request.authorUserName?.trim()
      || this.request.userName?.trim()
      || this.request.userId?.trim()
      || null;
  }

  openProfile(routeValue?: string | null): void {
    const target = routeValue?.trim();
    if (!target) return;

    this.router.navigate(['/profile', target]);
  }

  openRequestAuthorProfile(): void {
    const routeValue = this.getProfileRouteValue();
    if (!routeValue) return;

    this.router.navigate(['/profile', routeValue]);
  }

  get totalRequestedSum(): number {
    if (!this.request.requestItems?.length) return 0;

    return this.request.requestItems.reduce((sum, item) => {
      const qty = Number(item.quantity) || 0;
      const price = Number(item.unitPrice) || 0;
      return sum + qty * price;
    }, 0);
  }

  get collectedAmount(): number {
    return Number(this.request.totalPayments) || 0;
  }

  get progressPercent(): number {
    const total = this.totalRequestedSum;
    if (!total || total <= 0) return 0;

    const percent = (this.collectedAmount / total) * 100;
    return Math.max(0, Math.min(percent, 100));
  }

  toggleLike(): void {
    if (!this.currentUserId) return;

    const dto = { postId: this.request.id, userId: this.currentUserId };
    const previousState = this.isLiked;
    const previousCount = this.likesCount;

    this.isLiked = !this.isLiked;
    this.likesCount += this.isLiked ? 1 : -1;

    const request$ = this.isLiked
      ? this.likeService.likePost(dto)
      : this.likeService.unlikePost(dto);

    request$.subscribe({
      next: () => {},
      error: () => {
        this.isLiked = previousState;
        this.likesCount = previousCount;
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
        this.comments = data.map(c => ({
          ...c,
          userPhotoBase64: c.userPhotoBase64 || ''
        }));
        this.commentsCount = data.length;
        this.isLoadingComments = false;
      },
      error: () => {
        this.isLoadingComments = false;
      }
    });
  }

  sendComment(): void {
    if (!this.newCommentText.trim() || !this.currentUserId) return;

    const dto: CreatePostCommentDto = {
      postId: this.request.id,
      text: this.newCommentText.trim(),
      requestId: uuidv4()
    };

    this.commentService.addComment(dto).subscribe({
      next: () => {
        this.newCommentText = '';
        this.loadComments();
      },
      error: err => console.error('Помилка додавання коментаря', err)
    });
  }

  trackByRequestItem(_: number, item: HelpRequestItem): string {
    return item.id;
  }
}
