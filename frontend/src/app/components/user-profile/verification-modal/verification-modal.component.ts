import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VerificationService } from '../../../services/verification.service';
import {
  VerificationType, VerificationRequest,
  VERIFICATION_LABELS, VERIFICATION_ICONS
} from '../../../models/verification.model';

@Component({
  selector: 'app-verification-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verification-modal.component.html',
  styleUrls: ['./verification-modal.component.scss']
})
export class VerificationModalComponent implements OnInit {
  @Output() closed    = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<void>();

  existingRequest: VerificationRequest | null = null;
  selectedType: VerificationType = 1;
  notes       = '';
  loading     = true;
  isSubmitting = false;
  error       = '';
  success     = false;

  readonly types: { value: VerificationType; label: string; icon: string; desc: string }[] = [
    { value: 1, label: VERIFICATION_LABELS[1], icon: VERIFICATION_ICONS[1],
      desc: 'Для волонтерів — підтвердження участі в гуманітарній діяльності' },
    { value: 2, label: VERIFICATION_LABELS[2], icon: VERIFICATION_ICONS[2],
      desc: 'Для військовослужбовців ЗСУ або ветеранів' },
    { value: 3, label: VERIFICATION_LABELS[3], icon: VERIFICATION_ICONS[3],
      desc: 'Базова верифікація особистості' },
  ];

  constructor(private verificationService: VerificationService) {}

  ngOnInit(): void {
    this.verificationService.getMyRequest().subscribe({
      next: req  => { this.existingRequest = req; this.loading = false; },
      error: ()  => { this.loading = false; }
    });
  }

  get statusLabel(): string {
    switch (this.existingRequest?.status) {
      case 0:  return 'Очікує розгляду';
      case 1:  return 'Схвалено ✓';
      case 2:  return 'Відхилено';
      default: return '';
    }
  }

  get statusClass(): string {
    switch (this.existingRequest?.status) {
      case 0:  return 'status-pending';
      case 1:  return 'status-approved';
      case 2:  return 'status-rejected';
      default: return '';
    }
  }

  get canSubmitNew(): boolean {
    return !this.existingRequest || this.existingRequest.status === 2;
  }

  submit(): void {
    if (this.isSubmitting) return;
    this.isSubmitting = true;
    this.error = '';

    this.verificationService
      .submit({ verificationType: this.selectedType, notes: this.notes || undefined })
      .subscribe({
        next: () => {
          this.isSubmitting = false;
          this.success      = true;
          setTimeout(() => { this.submitted.emit(); this.closed.emit(); }, 1500);
        },
        error: (err) => {
          this.isSubmitting = false;
          this.error = typeof err?.error === 'string' ? err.error : 'Помилка при поданні заявки';
        }
      });
  }
}
