import { Component, OnInit, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';
import { MapFilterParams, SupportTypeDto } from '../../../models/map.models';

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
  isOpen = false;

  selectedTypeId   = '';
  isActive         = '';
  region           = '';
  search           = '';
  maxTargetAmount: number | null  = null;
  minCollectedAmount: number | null = null;

  get activeCount(): number {
    let count = 0;
    if (this.selectedTypeId)         count++;
    if (this.isActive !== '')         count++;
    if (this.region.trim())           count++;
    if (this.search.trim())           count++;
    if (this.maxTargetAmount != null) count++;
    if (this.minCollectedAmount != null) count++;
    return count;
  }

  ngOnInit(): void {
    this.http.get<SupportTypeDto[]>(`${environment.apiUrl}/SupportTypes`)
      .subscribe({
        next:  types => { this.supportTypes = types; this.loadingTypes = false; },
        error: ()    => { this.loadingTypes = false; }
      });
  }

  toggleOpen(): void {
    this.isOpen = !this.isOpen;
  }

  setStatus(val: string): void {
    this.isActive = val;
    this.applyFilters();
  }

  applyFilters(): void {
    const params: MapFilterParams = {};

    if (this.selectedTypeId) params.supportTypeId = this.selectedTypeId;
    if (this.isActive !== '') params.isActive = this.isActive === 'true';
    if (this.region.trim()) params.region = this.region.trim();
    if (this.search.trim()) params.search = this.search.trim();
    if (this.maxTargetAmount != null) params.maxTargetAmount = this.maxTargetAmount;
    if (this.minCollectedAmount != null) params.minCollectedAmount = this.minCollectedAmount;

    this.filtersChanged.emit(params);
  }
  resetFilters(): void {
    this.selectedTypeId      = '';
    this.isActive            = '';
    this.region              = '';
    this.search              = '';
    this.maxTargetAmount     = null;
    this.minCollectedAmount  = null;
    this.filtersChanged.emit({});
  }
}
