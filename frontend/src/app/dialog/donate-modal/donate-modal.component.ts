import {
  Component, Input, Output, EventEmitter, signal, inject, OnChanges
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../services/payment.service';
import { HelpRequest } from '../../models/help-request.model';
import { MapMarkerDto } from '../../models/map.models';

@Component({
  selector: 'app-donate-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './donate-modal.component.html',
  styleUrls: ['./donate-modal.component.scss']
})
export class DonateModalComponent implements OnChanges {
  /**
   * Для використання з карток (HelpRequest feed)
   */
  @Input() helpRequest: HelpRequest | null = null;

  /**
   * Для використання з карти (MapMarkerDto — один маркер = один RequestItem)
   */
  @Input() request: MapMarkerDto | null = null;

  @Output() closed = new EventEmitter<void>();

  private readonly paymentService = inject(PaymentService);

  readonly presets      = [200, 500, 1000, 2500, 5000];
  readonly amount       = signal(500);
  readonly comment      = signal('');
  readonly isProcessing = signal(false);
  readonly error        = signal('');

  // ─── Уніфіковані геттери для обох типів ─────────────────────────────────

  get title(): string {
    return this.helpRequest?.title ?? this.request?.title ?? '';
  }

  get region(): string {
    return this.helpRequest?.locationName ?? this.request?.locationName ?? '';
  }

  get targetAmount(): number {
    return this.helpRequest?.targetAmount ?? this.request?.targetAmount ?? 0;
  }

  get collected(): number {
    return this.helpRequest?.collectedAmount ?? this.request?.collectedAmount ?? 0;
  }

  get isActive(): boolean {
    return this.helpRequest?.isActive ?? this.request?.isActive ?? false;
  }

  /**
   * Завжди helpRequestId:
   * - з HelpRequest: це id самого запиту
   * - з MapMarkerDto: це helpRequestId (не requestItemId!)
   */
  get helpRequestId(): string {
    return this.helpRequest?.id ?? this.request?.helpRequestId ?? '';
  }

  get percent(): number {
    if (!this.targetAmount) return 0;
    return Math.min(100, Math.round(this.collected / this.targetAmount * 100));
  }

  get canDonate(): boolean {
    return this.isActive;
  }

  get typeLabel(): string {
    if (this.helpRequest?.requestItems?.length) {
      const types = [...new Set(this.helpRequest.requestItems.map(i => i.supportTypeName))];
      return types.slice(0, 2).join(', ') || 'Інше';
    }
    if (this.request?.supportTypeName) {
      return this.request.supportTypeName;
    }
    return 'Інше';
  }

  ngOnChanges(): void {
    this.error.set('');
    this.isProcessing.set(false);
  }

  donate(): void {
    if (!this.helpRequestId || !this.canDonate) return;
    if (this.amount() <= 0) {
      this.error.set('Введіть суму більше 0');
      return;
    }

    this.isProcessing.set(true);
    this.error.set('');

    this.paymentService.donate({
      helpRequestId: this.helpRequestId,
      amount:        this.amount(),
      comment:       this.comment() || `Донат SupportWay — ${this.title}`
    }).subscribe({
      next: res => {
        window.open(res.checkoutUrl, '_blank');
        this.isProcessing.set(false);
        this.closed.emit();
      },
      error: () => {
        this.error.set('Помилка при створенні платежу. Спробуйте ще раз.');
        this.isProcessing.set(false);
      }
    });
  }

  close(): void {
    this.closed.emit();
  }
}
