import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { VerificationService } from '../../../services/verification.service';
import {
  VerificationType,
  VerificationRequest,
  VERIFICATION_LABELS,
  VERIFICATION_TYPES
} from '../../../models/verification.model';

@Component({
  selector: 'app-verification-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './verification-modal.component.html',
  styleUrls: ['./verification-modal.component.scss']
})
export class VerificationModalComponent implements OnInit {
  @Output() closed = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<void>();

  existingRequest: VerificationRequest | null = null;

  selectedType: VerificationType = VERIFICATION_TYPES.Volunteer;
  notes = '';
  loading = true;
  isSubmitting = false;
  error = '';
  success = false;

  readonly VTYPE = VERIFICATION_TYPES;

  readonly types: { value: VerificationType; label: string; desc: string }[] = [
    {
      value: VERIFICATION_TYPES.Volunteer,
      label: VERIFICATION_LABELS[VERIFICATION_TYPES.Volunteer],
      desc: 'Підтвердження участі у волонтерській діяльності'
    },
    {
      value: VERIFICATION_TYPES.Military,
      label: VERIFICATION_LABELS[VERIFICATION_TYPES.Military],
      desc: 'Для військовослужбовців або ветеранів'
    }
  ];

  constructor(private verificationService: VerificationService) {}

  ngOnInit(): void {
    this.verificationService.getMyRequest().subscribe({
      next: req => {
        this.existingRequest = req;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  get statusLabel(): string {
    switch (this.existingRequest?.status) {
      case 0: return 'Очікує розгляду';
      case 1: return 'Схвалено';
      case 2: return 'Відхилено';
      default: return '';
    }
  }

  get statusClass(): string {
    switch (this.existingRequest?.status) {
      case 0: return 'status-pending';
      case 1: return 'status-approved';
      case 2: return 'status-rejected';
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

    this.verificationService.submit({
      verificationType: this.selectedType,
      notes: this.notes || undefined
    }).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.success = true;

        setTimeout(() => {
          this.submitted.emit();
          this.closed.emit();
        }, 1400);
      },
      error: (err) => {
        this.isSubmitting = false;
        this.error = typeof err?.error === 'string'
          ? err.error
          : 'Помилка при поданні заявки';
      }
    });
  }
}
