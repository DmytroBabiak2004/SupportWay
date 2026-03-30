import {
  Component, Output, EventEmitter, signal, computed, effect, inject, OnInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';
import { MapFilterParams, SupportTypeDto } from '../../models/map.models';

@Component({
  selector: 'app-map-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './map-filter.component.html',
  styleUrls: ['./map-filter.component.scss']
})
export class MapFilterComponent implements OnInit {
  @Output() filtersChanged = new EventEmitter<MapFilterParams>();

  private readonly http = inject(HttpClient);

  // Типи підтримки з бекенду (SupportTypesController вже існує)
  readonly supportTypes    = signal<SupportTypeDto[]>([]);
  readonly selectedTypeId  = signal<string | undefined>(undefined);
  readonly isActive        = signal<boolean | undefined>(true);
  readonly region          = signal('');
  readonly maxTarget       = signal<number | undefined>(undefined);
  readonly minCollected    = signal<number | undefined>(undefined);

  // computed збирає всі сигнали в один об'єкт фільтра
  readonly currentFilters = computed<MapFilterParams>(() => ({
    supportTypeId: this.selectedTypeId(),
    isActive:      this.isActive(),
    region:        this.region() || undefined,
    maxTarget:     this.maxTarget(),
    minCollected:  this.minCollected()
  }));

  constructor() {

    effect(() => {
      this.filtersChanged.emit(this.currentFilters());
    });
  }

  ngOnInit(): void {
        this.http.get<SupportTypeDto[]>(`${environment.apiUrl}/supporttypes`)
      .subscribe(types => this.supportTypes.set(types));
  }

  toggleType(id: string): void {
    this.selectedTypeId.set(this.selectedTypeId() === id ? undefined : id);
  }

  resetFilters(): void {
    this.selectedTypeId.set(undefined);
    this.isActive.set(true);
    this.region.set('');
    this.maxTarget.set(undefined);
    this.minCollected.set(undefined);
  }
}
