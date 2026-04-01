import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VerificationService } from '../../services/verification.service';
import { ProfileService } from '../../services/profile.service';
import {
  VerificationRequest, VerificationStatus,
  VERIFICATION_LABELS, VERIFICATION_ICONS
} from '../../models/verification.model';

@Component({
  selector: 'app-admin-verification',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-verification.component.html',
  styleUrls: ['./admin-verification.component.scss']
})
export class AdminVerificationComponent implements OnInit {
  requests: VerificationRequest[] = [];
  filtered: VerificationRequest[] = [];
  statusFilter: number | '' = '';
  selectedRequest: VerificationRequest | null = null;
  adminComment = '';
  loading = true;
  processing = false;
  error = '';
  successMsg = '';

  readonly LABELS = VERIFICATION_LABELS;
  readonly ICONS  = VERIFICATION_ICONS;

  constructor(
    private verificationService: VerificationService,
    public profileService: ProfileService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    const status = this.statusFilter !== '' ? (this.statusFilter as VerificationStatus) : undefined;
    this.verificationService.getAll(status).subscribe({
      next: list => {
        this.requests = list;
        this.filtered = list;
        this.loading  = false;
      },
      error: () => { this.loading = false; this.error = 'Не вдалося завантажити заявки'; }
    });
  }

  filterByStatus(): void {
    const s = this.statusFilter;
    this.filtered = s === '' ? [...this.requests] : this.requests.filter(r => r.status === +s);
  }

  openDetail(r: VerificationRequest): void {
    this.selectedRequest = r;
    this.adminComment = r.adminComment || '';
    this.error = '';
    this.successMsg = '';
  }

  decide(approved: boolean): void {
    if (!this.selectedRequest || this.processing) return;
    this.processing = true;
    this.verificationService.decide(this.selectedRequest.id, approved, this.adminComment || undefined)
      .subscribe({
        next: () => {
          this.processing = false;
          this.successMsg = approved ? 'Верифікацію схвалено ✓' : 'Заявку відхилено';
          this.selectedRequest = null;
          this.load();
        },
        error: () => {
          this.processing = false;
          this.error = 'Помилка при обробці заявки';
        }
      });
  }

  statusLabel(s: number): string {
    switch (s) { case 0: return 'Очікує'; case 1: return 'Схвалено'; case 2: return 'Відхилено'; default: return ''; }
  }
  statusClass(s: number): string {
    switch (s) { case 0: return 'badge-pending'; case 1: return 'badge-approved'; case 2: return 'badge-rejected'; default: return ''; }
  }
}
