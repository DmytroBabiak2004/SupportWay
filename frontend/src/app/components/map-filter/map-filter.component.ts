import {
  Component, OnInit, Output, EventEmitter, inject, signal
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

  private http = inject(HttpClient);

  supportTypes: SupportTypeDto[] = [];
  loadingTypes = true;

  // filter state
  selectedTypeId: string = '';
  isActive: string = '';     // '' | 'true' | 'false'
  region: string = '';
  maxTarget: number | null = null;
  minCollected: number | null = null;

  ngOnInit(): void {
    this.http.get<SupportTypeDto[]>(`${environment.apiUrl}/SupportTypes`)
      .subscribe({
        next: types => { this.supportTypes = types; this.loadingTypes = false; },
        error: () => { this.loadingTypes = false; }
      });
  }

  applyFilters(): void {
    const params: MapFilterParams = {};
    if (this.selectedTypeId) params.supportTypeId = this.selectedTypeId;
    if (this.isActive !== '') params.isActive = this.isActive === 'true';
    if (this.region.trim()) params.region = this.region.trim();
    if (this.maxTarget != null) params.maxTarget = this.maxTarget;
    if (this.minCollected != null) params.minCollected = this.minCollected;
    this.filtersChanged.emit(params);
  }

  resetFilters(): void {
    this.selectedTypeId = '';
    this.isActive       = '';
    this.region         = '';
    this.maxTarget      = null;
    this.minCollected   = null;
    this.filtersChanged.emit({});
  }
}
