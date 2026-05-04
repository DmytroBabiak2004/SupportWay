import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { VerificationService } from '../../services/verification.service';
import { ProfileService } from '../../services/profile.service';
import {
  VerificationRequest,
  VerificationStatus,
  VERIFICATION_LABELS,
  VERIFICATION_ICONS
} from '../../models/verification.model';

@Component({
  selector: 'app-admin-verification',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-verification.component.html',
  styleUrls: ['./admin-verification.component.scss']
})
export class AdminVerificationComponent implements OnInit, OnDestroy {
  private verificationService = inject(VerificationService);
  public profileService = inject(ProfileService);
  private router = inject(Router);

  requests: VerificationRequest[] = [];
  filtered: VerificationRequest[] = [];

  statusFilter: '' | '0' | '1' | '2' = '';

  selectedRequest: VerificationRequest | null = null;
  adminComment = '';

  loading = true;
  processing = false;

  error = '';
  successMsg = '';

  readonly LABELS = VERIFICATION_LABELS;
  readonly ICONS = VERIFICATION_ICONS;

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.unlockScroll();
  }

  get pendingCount(): number {
    return this.requests.filter(request => request.status === 0).length;
  }

  get approvedCount(): number {
    return this.requests.filter(request => request.status === 1).length;
  }

  get rejectedCount(): number {
    return this.requests.filter(request => request.status === 2).length;
  }

  load(): void {
    this.loading = true;
    this.error = '';

    const status = this.statusFilter !== ''
      ? Number(this.statusFilter) as VerificationStatus
      : undefined;

    this.verificationService.getAll(status).subscribe({
      next: list => {
        this.requests = list ?? [];
        this.filtered = [...this.requests];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.error = 'Не вдалося завантажити заявки';
      }
    });
  }

  filterByStatus(): void {
    if (this.statusFilter === '') {
      this.filtered = [...this.requests];
      return;
    }

    const status = Number(this.statusFilter);
    this.filtered = this.requests.filter(request => request.status === status);
  }

  openDetail(request: VerificationRequest): void {
    this.selectedRequest = request;
    this.adminComment = request.adminComment || '';
    this.error = '';
    this.successMsg = '';
    this.lockScroll();
  }

  closeDetail(): void {
    this.selectedRequest = null;
    this.adminComment = '';
    this.processing = false;
    this.unlockScroll();
  }

  decide(approved: boolean): void {
    if (!this.selectedRequest || this.processing) {
      return;
    }

    this.processing = true;
    this.error = '';

    this.verificationService
      .decide(this.selectedRequest.id, approved, this.adminComment || undefined)
      .subscribe({
        next: () => {
          this.processing = false;
          this.successMsg = approved
            ? 'Верифікацію схвалено'
            : 'Заявку відхилено';

          this.closeDetail();
          this.load();
        },
        error: () => {
          this.processing = false;
          this.error = 'Помилка при обробці заявки';
        }
      });
  }

  openProfile(userIdOrUsername?: string | null, event?: MouseEvent): void {
    event?.stopPropagation();

    const value = (userIdOrUsername || '').trim();

    if (!value) {
      return;
    }

    this.router.navigate(['/profile', value]);
  }

  getAvatar(request: VerificationRequest | null | undefined): string | null {
    if (!request?.photoBase64) {
      return null;
    }

    return this.profileService.getAvatarSrc(request.photoBase64);
  }

  getInitial(username?: string | null): string {
    const value = username?.trim();

    if (!value) {
      return '?';
    }

    return value.charAt(0).toUpperCase();
  }

  getTypeLabel(type: number): string {
    return this.LABELS[type] || 'Невідомо';
  }

  getTypeIcon(type: number): string {
    return this.ICONS[type] || '•';
  }

  getTypeClass(type: number): string {
    switch (type) {
      case 1:
        return 'type-volunteer';
      case 2:
        return 'type-military';
      default:
        return 'type-user';
    }
  }

  getStatusDescription(status: number): string {
    switch (status) {
      case 0:
        return 'Заявка очікує рішення адміністратора';
      case 1:
        return 'Заявку схвалено, роль користувача вже оновлено';
      case 2:
        return 'Заявку відхилено адміністратором';
      default:
        return '';
    }
  }

  statusLabel(status: number): string {
    switch (status) {
      case 0:
        return 'Очікує';
      case 1:
        return 'Схвалено';
      case 2:
        return 'Відхилено';
      default:
        return '';
    }
  }

  statusClass(status: number): string {
    switch (status) {
      case 0:
        return 'badge-pending';
      case 1:
        return 'badge-approved';
      case 2:
        return 'badge-rejected';
      default:
        return '';
    }
  }

  trackByRequestId(_: number, request: VerificationRequest): string {
    return request.id;
  }

  private lockScroll(): void {
    if (typeof document !== 'undefined') {
      document.body.style.overflow = 'hidden';
    }
  }

  private unlockScroll(): void {
    if (typeof document !== 'undefined') {
      document.body.style.overflow = '';
    }
  }
}
