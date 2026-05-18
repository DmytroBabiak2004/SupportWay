import {
  Component, Input, Output, EventEmitter,
  signal, inject, OnChanges, SimpleChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../services/payment.service';
import { HelpRequest, HelpRequestDetails } from '../../models/help-request.model';
import { DonateResponseDto, MapMarkerDto } from '../../models/map.models';

type Step = 'amount' | 'jar_pay' | 'card_only';

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

  @Output() closed  = new EventEmitter<void>();
  @Output() donated = new EventEmitter<number>();

  private readonly paymentService = inject(PaymentService);

  readonly presets: number[] = [100, 200, 500, 1000, 2500];

  readonly amount        = signal<number>(500);
  readonly comment       = signal<string>('');
  readonly isProcessing  = signal<boolean>(false);
  readonly error         = signal<string>('');
  readonly step          = signal<Step>('amount');
  readonly copyFeedback  = signal<string>('');

  readonly paymentLink   = signal<string | null>(null);
  readonly cardNumber    = signal<string | null>(null);
  readonly recipientName = signal<string | null>(null);
  readonly instructions  = signal<string | null>(null);
  readonly paidAmount    = signal<number>(0);

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
    return Math.max(0, Math.min(100, Math.round((this.collected / this.targetAmount) * 100)));
  }

  get canDonate(): boolean {
    return this.isActive && !!this.helpRequestId;
  }

  get typeLabel(): string {
    const items = this.details?.requestItems ?? this.helpRequest?.requestItems;
    if (items?.length) {
      const unique: string[] = [];
      for (const item of items) {
        const t = item.supportTypeName?.trim();
        if (t && !unique.includes(t)) unique.push(t);
      }
      if (unique.length) return unique.slice(0, 2).join(', ');
    }
    return this.request?.supportTypeName?.trim() || 'Інше';
  }

  ngOnChanges(changes: SimpleChanges): void {
    this.reset();
    if (changes['helpRequest'] || changes['request'] || changes['details'])
      this.amount.set(this.getInitialAmount());
  }

  private reset(): void {
    this.error.set('');
    this.isProcessing.set(false);
    this.comment.set('');
    this.step.set('amount');
    this.paymentLink.set(null);
    this.cardNumber.set(null);
    this.recipientName.set(null);
    this.instructions.set(null);
    this.paidAmount.set(0);
    this.copyFeedback.set('');
  }

  prepare(): void {
    if (!this.canDonate) { this.error.set('Збір недоступний.'); return; }

    const currentAmount = Number(this.amount());
    if (!Number.isFinite(currentAmount) || currentAmount <= 0) {
      this.error.set('Введіть суму більше 0.');
      return;
    }

    this.isProcessing.set(true);
    this.error.set('');

    this.paymentService.donate({
      helpRequestId: this.helpRequestId,
      amount: currentAmount,
      comment: this.comment().trim() || `Донат SupportWay — ${this.title}`
    }).subscribe({
      next: (res: DonateResponseDto) => {
        this.isProcessing.set(false);
        this.paidAmount.set(currentAmount);
        this.recipientName.set(res.recipientName ?? null);
        this.instructions.set(res.instructions ?? null);
        this.cardNumber.set(res.cardNumber ?? null);

        if (res.paymentLink) {
          this.paymentLink.set(res.paymentLink);
          this.step.set('jar_pay');
        } else {
          this.step.set('card_only');
        }

        this.donated.emit(currentAmount);
      },
      error: (err) => {
        const msg = typeof err?.error === 'string' ? err.error : err?.error?.message || '';
        this.error.set(msg || 'Не вдалося підготувати реквізити. Спробуйте ще раз.');
        this.isProcessing.set(false);
      }
    });
  }

  openJar(): void {
    const link = this.paymentLink();
    if (!link) return;
    window.open(link, '_blank', 'noopener,noreferrer');
  }

  async copyCard(): Promise<void> {
    const card = this.cardNumber();
    if (!card) return;
    try {
      await navigator.clipboard.writeText(card);
      this.copyFeedback.set('✓ Скопійовано');
    } catch {
      this.copyFeedback.set('Помилка копіювання');
    }
    setTimeout(() => this.copyFeedback.set(''), 2000);
  }

  backToAmount(): void { this.step.set('amount'); this.error.set(''); }
  close(): void { this.closed.emit(); }

  private getInitialAmount(): number {
    if (this.targetAmount > 0)
      return Math.min(this.targetAmount, this.presets[1] ?? this.presets[0]);
    return this.presets[1] ?? this.presets[0];
  }
}
