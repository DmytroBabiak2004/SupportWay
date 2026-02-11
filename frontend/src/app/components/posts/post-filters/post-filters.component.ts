import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-post-filters', // Зверни увагу на селектор
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './post-filters.component.html',
  styleUrls: ['./post-filters.component.scss']
})
export class PostFiltersComponent {
  @Input() searchQuery = '';
  @Input() sortBy: 'newest' | 'oldest' = 'newest';

  @Output() searchQueryChange = new EventEmitter<string>();
  @Output() sortByChange = new EventEmitter<'newest' | 'oldest'>();
  @Output() create = new EventEmitter<void>();
  @Output() filterChanged = new EventEmitter<void>();

  onQueryChange(val: string) {
    this.searchQuery = val; // Оновлюємо локально для ngModel
    this.searchQueryChange.emit(val);
    this.filterChanged.emit();
  }

  onSortChange(val: any) {
    this.sortBy = val;
    this.sortByChange.emit(val);
    this.filterChanged.emit();
  }
}
