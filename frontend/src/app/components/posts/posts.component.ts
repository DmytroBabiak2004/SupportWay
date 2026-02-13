import { Component, OnInit, AfterViewInit, OnDestroy, ViewChild, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

// Components
import { PostCardComponent } from './post-card/post-card.component';
import { CreatePostModalComponent } from '../../dialog/create-post-modal/create-post-modal.component';

// Models & Services
import { PostService } from '../../services/post.service';
import { Post } from '../../models/post.model';

@Component({
  selector: 'app-posts',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule, // Потрібен для ngModel на інпутах
    PostCardComponent,
    CreatePostModalComponent
  ],
  templateUrl: './posts.component.html',
  styleUrls: ['./posts.component.scss']
})
export class PostsComponent implements OnInit, AfterViewInit, OnDestroy {
  private postService = inject(PostService);
  private router = inject(Router);

  @ViewChild('scrollAnchor') scrollAnchor!: ElementRef;
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  // --- Data ---
  rawPosts: Post[] = [];        // Усі завантажені пости
  filteredPosts: Post[] = [];   // Пости після пошуку/сортування
  displayPosts: Post[] = [];    // Пости для відображення (пагінація)

  // --- UI State ---
  viewMode: 'feed' | 'user' = 'feed';
  isLoading = false;
  isLoadingMore = false;
  isModalOpen = false;

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

  // --- Data Loading ---
  loadData() {
    this.isLoading = true;
    const request$ = this.viewMode === 'feed'
      ? this.postService.getFeed()
      : this.postService.getMyPosts();

    request$.subscribe({
      next: (posts) => {
        this.rawPosts = posts;
        this.applyFilters(); // Одразу застосовуємо фільтри
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        // Тут можна додати обробку помилок
      }
    });
  }

  // --- Filter & Sort Logic ---
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

    this.filteredPosts = result;

    // 3. Скидання пагінації при зміні фільтрів
    this.currentLimit = this.PAGE_SIZE;
    this.updateDisplay();

    // Скрол вгору, якщо це не перше завантаження
    if (!this.isLoading) this.scrollToTop();
  }

  onSearchChange(val: string) {
    this.searchQuery = val;
    this.applyFilters();
  }

  onSortChange(val: any) {
    this.sortBy = val;
    this.applyFilters();
  }

  private updateDisplay() {
    this.displayPosts = this.filteredPosts.slice(0, this.currentLimit);
  }

  // --- Infinite Scroll ---
  private initObserver() {
    if (!this.scrollAnchor) return;

    this.observer = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting) {
        this.loadMore();
      }
    }, {
      root: null, // null означає viewport браузера
      rootMargin: '200px' // Починати вантажити за 200px до кінця
    });

    this.observer.observe(this.scrollAnchor.nativeElement);
  }

  loadMore() {
    if (this.isLoadingMore || this.displayPosts.length >= this.filteredPosts.length) return;

    this.isLoadingMore = true;

    // Імітація мережевої затримки для плавності UI
    setTimeout(() => {
      this.currentLimit += this.PAGE_SIZE;
      this.updateDisplay();
      this.isLoadingMore = false;
    }, 300);
  }

  private scrollToTop() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // --- Actions ---
  setView(mode: 'feed' | 'user') {
    if (this.viewMode === mode) return;
    this.viewMode = mode;
    this.searchQuery = ''; // Очищаємо пошук при зміні таба
    this.loadData();
  }

  openProfile(post: Post) {
    const id = post.authorUserName || post.userId;
    if (id) this.router.navigate(['/profile', id]);
  }

  trackByPostId(_: number, post: Post) {
    return post.id;
  }

  // Modal
  openModal() { this.isModalOpen = true; }
  closeModal() { this.isModalOpen = false; }
  onPostCreated() {
    this.closeModal();
    this.loadData();
  }
}
