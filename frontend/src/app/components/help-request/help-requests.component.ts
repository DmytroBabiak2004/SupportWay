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
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { HelpRequestCardComponent } from './help-request-card/help-request-card.component';
import { CreateHelpRequestModalComponent } from '../../dialog/create-help-request-modal/create-help-request-modal.component';

import { HelpRequestService } from '../../services/help-request.service';
import { HelpRequest } from '../../models/help-request.model';

@Component({
  selector: 'app-help-requests',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HelpRequestCardComponent,
    CreateHelpRequestModalComponent
  ],
  templateUrl: './help-requests.component.html',
  styleUrls: ['./help-requests.component.scss']
})
export class HelpRequestsComponent implements OnInit, AfterViewInit, OnDestroy {
  private helpRequestService = inject(HelpRequestService);
  private router = inject(Router);

  @ViewChild('scrollAnchor') scrollAnchor!: ElementRef;

  rawRequests: HelpRequest[] = [];
  filteredRequests: HelpRequest[] = [];
  displayRequests: HelpRequest[] = [];

  viewMode: 'feed' | 'user' = 'feed';
  isLoading = false;
  isLoadingMore = false;
  isModalOpen = false;

  searchQuery = '';
  sortBy: 'newest' | 'oldest' = 'newest';

  private observer: IntersectionObserver | undefined;
  private readonly PAGE_SIZE = 10;
  private currentLimit = 10;

  ngOnInit(): void {
    this.loadData();
  }

  ngAfterViewInit(): void {
    this.initObserver();
  }

  ngOnDestroy(): void {
    this.observer?.disconnect();
  }

  loadData(): void {
    this.isLoading = true;

    const request$ = this.viewMode === 'feed'
      ? this.helpRequestService.getFeed?.(1, 100) ?? this.helpRequestService.getMyHelpRequests(1, 100)
      : this.helpRequestService.getMyHelpRequests(1, 100);

    request$.subscribe({
      next: (requests: HelpRequest[]) => {
        this.rawRequests = requests ?? [];
        this.applyFilters();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Помилка завантаження help requests', err);
        this.isLoading = false;
      }
    });
  }

  applyFilters(): void {
    let result = [...this.rawRequests];

    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();

      result = result.filter(r =>
        r.content?.toLowerCase().includes(q) ||
        r.title?.toLowerCase().includes(q) ||
        r.userName?.toLowerCase().includes(q) ||
        r.authorUserName?.toLowerCase().includes(q) ||
        r.locationName?.toLowerCase().includes(q) ||
        r.requestItems?.some(item =>
          item.name?.toLowerCase().includes(q) ||
          item.supportTypeName?.toLowerCase().includes(q)
        )
      );
    }

    result.sort((a, b) => {
      const dateA = new Date(a.createdAt).getTime();
      const dateB = new Date(b.createdAt).getTime();
      return this.sortBy === 'newest' ? dateB - dateA : dateA - dateB;
    });

    this.filteredRequests = result;
    this.currentLimit = this.PAGE_SIZE;
    this.updateDisplay();

    if (!this.isLoading) {
      this.scrollToTop();
    }
  }

  onSearchChange(val: string): void {
    this.searchQuery = val;
    this.applyFilters();
  }

  onSortChange(val: 'newest' | 'oldest'): void {
    this.sortBy = val;
    this.applyFilters();
  }

  private updateDisplay(): void {
    this.displayRequests = this.filteredRequests.slice(0, this.currentLimit);
  }

  private initObserver(): void {
    if (!this.scrollAnchor) return;

    this.observer = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting) {
        this.loadMore();
      }
    }, {
      root: null,
      rootMargin: '200px'
    });

    this.observer.observe(this.scrollAnchor.nativeElement);
  }

  loadMore(): void {
    if (this.isLoadingMore || this.displayRequests.length >= this.filteredRequests.length) {
      return;
    }

    this.isLoadingMore = true;

    setTimeout(() => {
      this.currentLimit += this.PAGE_SIZE;
      this.updateDisplay();
      this.isLoadingMore = false;
    }, 300);
  }

  private scrollToTop(): void {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  setView(mode: 'feed' | 'user'): void {
    if (this.viewMode === mode) return;

    this.viewMode = mode;
    this.searchQuery = '';
    this.loadData();
  }

  openProfile(request: HelpRequest): void {
    const id = request.authorUserName || request.userName || request.userId;
    if (id) {
      this.router.navigate(['/profile', id]);
    }
  }

  trackByRequestId(_: number, request: HelpRequest): string {
    return request.id;
  }

  openModal(): void {
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  onRequestCreated(): void {
    this.closeModal();
    this.loadData();
  }
}
