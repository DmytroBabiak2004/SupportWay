import { Component, Input, Output, EventEmitter, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Post } from '../../../models/post.model';
import { ProfileService } from '../../../services/profile.service';
import { PostLikeService } from '../../../services/post-like.service';
import { PostCommentService } from '../../../services/post-comment.service';
import { PostComment, CreatePostCommentDto } from '../../../models/post-comment.model';
import { AuthService } from '../../../services/auth.service';
import { filter } from 'rxjs/operators';
import { RelativeTimePipe } from '../../../pipes/relative-time.pipe';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [CommonModule, FormsModule, RelativeTimePipe],
  templateUrl: './post-card.component.html',
  styleUrls: ['./post-card.component.scss']
})
export class PostCardComponent implements OnInit {
  public profileService = inject(ProfileService);
  private likeService = inject(PostLikeService);
  private commentService = inject(PostCommentService);
  private authService = inject(AuthService);

  @Input({ required: true }) post!: Post;
  @Output() profileClick = new EventEmitter<Post>();

  isLiked = false;
  likesCount = 0;
  commentsCount = 0;

  showComments = false;
  comments: PostComment[] = [];
  newCommentText = '';
  isLoadingComments = false;
  currentUserId = '';

  ngOnInit() {
    // Ініціалізація лайків і коментарів з бекенду
    this.likesCount = this.post.likesCount || 0;
    this.commentsCount = this.post.commentsCount || 0;
    this.isLiked = this.post.isLikedByCurrentUser || false;


    // Чекаємо, поки AuthService завантажить користувача
    this.authService.getUserInfo$()
      .pipe(filter(user => !!user))
      .subscribe(user => {
        this.currentUserId = user!.id;

        // Якщо бекенд не передав isLiked, можна перевірити тут
        if (this.post.isLikedByCurrentUser === undefined) {
          this.checkIfUserLiked();
        }
      });
  }

  private checkIfUserLiked() {
    if (!this.currentUserId) return;
    this.likeService.getLikesCount(this.post.id).subscribe(count => {
    });
  }

  getInitials(name: string): string {
    return name ? name[0].toUpperCase() : '?';
  }

  toggleLike() {
    if (!this.currentUserId) return; // захист від незалогіненого

    const dto = { postId: this.post.id, userId: this.currentUserId };

    // Оптимістичне оновлення UI
    const previousState = this.isLiked;
    const previousCount = this.likesCount;

    this.isLiked = !this.isLiked;
    this.likesCount += this.isLiked ? 1 : -1;

    const request$ = this.isLiked
      ? this.likeService.likePost(dto)
      : this.likeService.unlikePost(dto);

    request$.subscribe({
      next: () => {
        // Успішно, залишаємо як є
      },
      error: () => {
        // Повертаємо попередній стан при помилці
        this.isLiked = previousState;
        this.likesCount = previousCount;
      }
    });
  }

  toggleComments() {
    this.showComments = !this.showComments;
    if (this.showComments && this.comments.length === 0) this.loadComments();
  }

  loadComments() {
    this.isLoadingComments = true;
    this.commentService.getCommentsByPost(this.post.id).subscribe({
      next: data => {
        this.comments = data;
        this.commentsCount = data.length;
        this.isLoadingComments = false;
      },
      error: () => this.isLoadingComments = false
    });
  }

  sendComment() {
    if (!this.newCommentText.trim()) return;
    if (!this.currentUserId) {
      alert('Ви не залогінені!');
      return;
    }

    const dto: CreatePostCommentDto = {
      postId: this.post.id,
      text: this.newCommentText,
      requestId: crypto.randomUUID()
    };

    this.commentService.addComment(dto).subscribe({
      next: () => {
        this.newCommentText = '';
        this.loadComments();
      },
      error: err => {
        console.error('Помилка при додаванні коментаря', err);
      }
    });
  }
}
