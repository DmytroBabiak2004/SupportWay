import { Component, OnDestroy, OnInit, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError, tap, takeUntil } from 'rxjs/operators';

import { UserSearchService } from '../../../services/user-search.service';
import { ProfileService } from '../../../services/profile.service';
import { UserSearch } from '../../../models/user-search.model';

@Component({
  selector: 'app-user-search',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-search.component.html',
  styleUrls: ['./user-search.component.scss']
})
export class UserSearchComponent implements OnInit, OnDestroy {
  searchText = '';
  results: UserSearch[] = [];
  isLoading = false;
  isFocused = false; // Повертаємо стан фокусу

  private search$ = new Subject<string>();
  private destroy$ = new Subject<void>();

  constructor(
    private userSearchService: UserSearchService,
    public profileService: ProfileService,
    private router: Router,
    private eRef: ElementRef // Потрібно для HostListener
  ) {}

  // Закриваємо випадаючий список при кліку поза пошуком
  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.isFocused = false;
    }
  }

  ngOnInit(): void {
    this.search$.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      tap(term => {
        const cleanTerm = term.trim();
        if (cleanTerm) {
          this.isLoading = true;
        } else {
          this.results = [];
          this.isLoading = false;
        }
      }),
      switchMap(term => {
        const cleanTerm = term.trim();
        if (!cleanTerm) {
          return of([]);
        }
        return this.userSearchService.searchUsers(cleanTerm).pipe(
          catchError(() => of([])),
          tap(() => this.isLoading = false) // Вимикаємо Loader після отримання даних
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe(users => {
      this.results = users;
      this.isLoading = false;
    });
  }

  onSearchInput(): void {
    this.isFocused = true;
    this.search$.next(this.searchText);
  }

  onFocus(): void {
    this.isFocused = true;
  }

  clearSearch(): void {
    this.searchText = '';
    this.results = [];
    this.isLoading = false;
    this.isFocused = false;
  }

  get showDropdown(): boolean {
    // Показуємо список, якщо є текст, йде завантаження або вже є результати
    return this.isFocused && (this.results.length > 0 || this.isLoading || (this.searchText.trim().length > 0));
  }

  getInitials(user: UserSearch): string {
    const name = user.name?.trim() || '';
    const fullName = user.fullName?.trim() || '';
    const userName = user.userName?.trim() || '';

    if (name || fullName) {
      return ((name[0] || '') + (fullName[0] || '')).toUpperCase();
    }
    return (userName[0] || '?').toUpperCase();
  }

  openProfile(user: UserSearch): void {
    const identifier = user.userName || user.id;
    if (identifier) {
      this.router.navigate(['/profile', identifier]);
      this.clearSearch();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
