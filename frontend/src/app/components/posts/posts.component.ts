import {
  Component,
  OnInit,
  AfterViewInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  inject
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

// Components
import { PostCardComponent } from './post-card/post-card.component';
import { PostFiltersComponent } from './post-filters/post-filters.component';
import { CreatePostModalComponent } from '../../dialog/create-post-modal/create-post-modal.component';

// Models & Services
import { PostService } from '../../services/post.service';
import { Post } from '../../models/post.model';

@Component({
  selector: 'app-posts',
  standalone: true,
  imports: [
    CommonModule,
    PostCardComponent,
    PostFiltersComponent,
    CreatePostModalComponent // ✅ Додано модалку
  ],
  templateUrl: './posts.component.html',
  styleUrls: ['./posts.component.scss']
})
export class PostsComponent implements OnInit, AfterViewInit, OnDestroy {

  private postService = inject(PostService);
  private router = inject(Router); // ✅ Додано роутер

  @ViewChild('scrollAnchor') scrollAnchor!: ElementRef;
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  // --- Data ---
  rawPosts: Post[] = [];        // Оригінальні дані з серверу
  filteredAllPosts: Post[] = []; // Відфільтровані дані
  displayPosts: Post[] = [];     // Дані для відображення (після пагінації)

  // --- State ---
  viewMode: 'feed' | 'user' = 'feed';
  isLoading = false;      // ✅ Додано лоадер
  isLoadingMore = false;
  isModalOpen = false;    // ✅ Додано стан модалки

  // --- Filters ---
  searchQuery = '';
  sortBy: 'newest' | 'oldest' = 'newest';

  // --- Pagination ---
  private observer: IntersectionObserver | undefined;
  private readonly PAGE_SIZE = 10;
  private currentLimit = 10;

  ngOnInit() {
    this.loadData();
  }

  ngAfterViewInit() {
    this.initObserver();
  }

  ngOnDestroy() {
    this.observer?.disconnect();
  }

  /* ================= DATA & FILTERS ================= */

// В методі loadData
  loadData() {
    if (this.rawPosts.length === 0) this.isLoading = true; // Тільки для першого завантаження

    const request$ = this.viewMode === 'feed'
      ? this.postService.getFeed()
      : this.postService.getMyPosts();

    request$.pipe(
          ).subscribe({
      next: (posts) => {
        this.rawPosts = posts;
        this.applyFilters();
        this.isLoading = false;
      }
    });
  }
  // ✅ Головна логіка фільтрації
  applyFilters() {
    let result = [...this.rawPosts];

    // 1. Пошук
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(p =>
        p.content?.toLowerCase().includes(q) ||
        p.title?.toLowerCase().includes(q) ||
        p.authorUserName?.toLowerCase().includes(q)
      );
    }

    // 2. Сортування
    result.sort((a, b) => {
      const dateA = new Date(a.createdAt).getTime();
      const dateB = new Date(b.createdAt).getTime();
      return this.sortBy === 'newest' ? dateB - dateA : dateA - dateB;
    });

    this.filteredAllPosts = result;

    // 3. Скидання пагінації
    this.currentLimit = this.PAGE_SIZE;
    this.updateDisplay();
    this.scrollToTop();
  }

  private updateDisplay() {
    this.displayPosts = this.filteredAllPosts.slice(0, this.currentLimit);
  }

  /* ================= SCROLL & OBSERVER ================= */

  private initObserver() {
    // Чекаємо поки елементи з'являться
    if (!this.scrollContainer || !this.scrollAnchor) return;

    this.observer = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting) {
        this.loadMore();
      }
    }, {
      root: this.scrollContainer.nativeElement, // Скролимо всередині контейнера
      rootMargin: '150px'
    });

    this.observer.observe(this.scrollAnchor.nativeElement);
  }

  loadMore() {
    // Не вантажимо, якщо вже йде процес або всі пости показані
    if (this.isLoadingMore || this.displayPosts.length >= this.filteredAllPosts.length) return;

    this.isLoadingMore = true;

    setTimeout(() => {
      this.currentLimit += this.PAGE_SIZE;
      this.updateDisplay();
      this.isLoadingMore = false;
    }, 200);
  }

  private scrollToTop() {
    if (this.scrollContainer) {
      this.scrollContainer.nativeElement.scrollTo({
        top: 0,
        behavior: 'auto'
      });
    }
  }

  /* ================= UI ACTIONS ================= */

  setView(mode: 'feed' | 'user') {
    if (this.viewMode === mode) return;
    this.viewMode = mode;
    this.loadData();
  }

  openProfile(post: Post) {
    const id = post.authorUserName || post.userId;
    if (id) this.router.navigate(['/profile', id]);
  }

  trackByPostId(_: number, post: Post) {
    return post.id;
  }

  // ✅ Modal Logic
  openModal() { this.isModalOpen = true; }
  closeModal() { this.isModalOpen = false; }
  onPostCreated() {
    this.closeModal();
    this.loadData(); // Перезавантажити стрічку після створення
  }
}
