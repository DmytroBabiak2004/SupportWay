import {
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  inject,
  OnChanges,
  SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../services/payment.service';
import { HelpRequest, HelpRequestDetails } from '../../models/help-request.model';
import { DonateResponseDto, MapMarkerDto } from '../../models/map.models';

@Component({
  selector: 'app-donate-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './donate-modal.component.html',
  styleUrls: ['./donate-modal.component.scss']
})
export class DonateModalComponent implements OnChanges {
  @Input() helpRequest: HelpRequest | null = null;
  @Input() details: HelpRequestDetails | null = null;
  @Input() request: MapMarkerDto | null = null;

  @Output() closed = new EventEmitter<void>();

  private readonly paymentService = inject(PaymentService);

  readonly presets: number[] = [200, 500, 1000, 2500, 5000];
  readonly amount = signal<number>(500);
  readonly comment = signal<string>('');
  readonly isProcessing = signal<boolean>(false);
  readonly error = signal<string>('');
  readonly paymentResponse = signal<DonateResponseDto | null>(null);
  readonly copyFeedback = signal<string>('');

  get title(): string {
    return this.details?.title ?? this.helpRequest?.title ?? this.request?.title ?? '';
  }

  get region(): string {
    return this.details?.locationName ?? this.helpRequest?.locationName ?? this.request?.locationName ?? '';
  }

  get targetAmount(): number {
    return this.details?.targetAmount ?? this.helpRequest?.targetAmount ?? this.request?.targetAmount ?? 0;
  }

  get collected(): number {
    return this.details?.collectedAmount ?? this.helpRequest?.collectedAmount ?? this.request?.collectedAmount ?? 0;
  }

  get isActive(): boolean {
    return this.details?.isActive ?? this.helpRequest?.isActive ?? this.request?.isActive ?? false;
  }

  get helpRequestId(): string {
    return this.details?.id ?? this.helpRequest?.id ?? this.request?.helpRequestId ?? '';
  }

  get percent(): number {
    if (this.targetAmount <= 0) return 0;
    const rawPercent = (this.collected / this.targetAmount) * 100;
    return Math.max(0, Math.min(100, Math.round(rawPercent)));
  }

  get canDonate(): boolean {
    return this.isActive && !!this.helpRequestId;
  }

  get typeLabel(): string {
    const sourceItems = this.details?.requestItems ?? this.helpRequest?.requestItems;
    if (sourceItems?.length) {
      const uniqueTypes: string[] = [];
      for (const item of sourceItems) {
        const typeName = item.supportTypeName?.trim();
        if (typeName && !uniqueTypes.includes(typeName)) uniqueTypes.push(typeName);
      }
      if (uniqueTypes.length > 0) return uniqueTypes.slice(0, 2).join(', ');
    }
    if (this.request?.supportTypeName?.trim()) return this.request.supportTypeName.trim();
    return 'Інше';
  }

  get donationDestinationLabel(): string {
    const response = this.paymentResponse();
    if (!response) return '';
    switch (response.paymentMethod) {
      case 'payment_link': return 'Платіжне посилання';
      case 'iban': return 'IBAN отримувача';
      case 'bank_card': return 'Картка отримувача';
      default: return 'Реквізити отримувача';
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.error.set('');
    this.isProcessing.set(false);
    this.comment.set('');
    this.paymentResponse.set(null);
    this.copyFeedback.set('');

    if (changes['helpRequest'] || changes['request'] || changes['details']) {
      this.amount.set(this.getInitialAmount());
    }
  }

  donate(): void {
    if (!this.canDonate) {
      this.error.set('Цей збір зараз недоступний для донату.');
      return;
    }

    const currentAmount = Number(this.amount());
    if (!Number.isFinite(currentAmount) || currentAmount <= 0) {
      this.error.set('Введіть коректну суму більше 0.');
      return;
    }

    this.isProcessing.set(true);
    this.error.set('');
    this.paymentResponse.set(null);

    this.paymentService.donate({
      helpRequestId: this.helpRequestId,
      amount: currentAmount,
      comment: this.comment().trim() || `Донат SupportWay — ${this.title}`
    }).subscribe({
      next: (res) => {
        this.paymentResponse.set(res);
        this.isProcessing.set(false);

        if (res?.paymentLink) {
          // Для ручного донату відкриваємо посилання лише якщо його явно повернули.
          // Для картки/IBAN користувач просто копіює реквізити нижче.
        }
      },
      error: (err) => {
        const backendMessage = typeof err?.error === 'string' ? err.error : err?.error?.message || '';
        this.error.set(backendMessage || 'Не вдалося підготувати реквізити для донату. Спробуйте ще раз.');
        this.isProcessing.set(false);
      }
    });
  }

  async copyValue(value?: string | null): Promise<void> {
    if (!value) return;
    try {
      await navigator.clipboard.writeText(value);
      this.copyFeedback.set('Скопійовано');
      setTimeout(() => this.copyFeedback.set(''), 1500);
    } catch {
      this.copyFeedback.set('Не вдалося скопіювати');
      setTimeout(() => this.copyFeedback.set(''), 1500);
    }
  }

  close(): void {
    this.closed.emit();
  }

  private getInitialAmount(): number {
    if (this.presets.length === 0) return 100;
    if (this.targetAmount > 0) {
      const suggested = Math.min(this.targetAmount, this.presets[0]);
      return suggested > 0 ? suggested : this.presets[0];
    }
    return this.presets[1] ?? this.presets[0];
  }
}
