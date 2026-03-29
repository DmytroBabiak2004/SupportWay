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
    this.likesCount = this.request.likesCount || 0;
    this.commentsCount = this.request.commentsCount || 0;
    this.isLiked = this.request.isLikedByCurrentUser || false;

    this.authService.getUserInfo$()
      .pipe(filter(user => !!user))
      .subscribe(user => {
        this.currentUserId = user!.id;
      });
  }

  openProfile(usernameOrId: string | undefined): void {
    if (!usernameOrId) return;
    this.router.navigate(['/profile', usernameOrId]);
  }

  getInitials(name: string): string {
    return name ? name[0].toUpperCase() : '?';
  }

  get totalRequestedSum(): number {
    if (!this.request.requestItems?.length) return 0;

    return this.request.requestItems.reduce((sum, item) => {
      return sum + (item.quantity * item.unitPrice);
    }, 0);
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
      text: this.newCommentText,
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
