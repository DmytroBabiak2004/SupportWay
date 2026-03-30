import {
  Component, Input, Output, EventEmitter, signal, inject, computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../services/payment.service';
import { RequestMapDto, getPrimaryTypeStyle } from '../../models/map.models';

@Component({
  selector: 'app-donate-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './donate-modal.component.html',
  styleUrls: ['./donate-modal.component.scss']
})
export class DonateModalComponent {
  @Input() request: RequestMapDto | null = null;
  @Output() closed = new EventEmitter<void>();

  private readonly paymentService = inject(PaymentService);

  readonly presets      = [200, 500, 1000, 2500, 5000];
  readonly amount       = signal(500);
  readonly comment      = signal('');
  readonly isProcessing = signal(false);
  readonly error        = signal('');

  readonly typeStyle = computed(() =>
    getPrimaryTypeStyle(this.request?.supportTypes ?? [])
  );

  readonly percent = computed(() => {
    const r = this.request;
    if (!r || r.targetAmount === 0) return 0;
    return Math.min(100, Math.round((r.collectedAmount / r.targetAmount) * 100));
  });

  readonly typeLabels = computed(() =>
    this.request?.supportTypes.map(t => t.nameOfType).join(', ') || 'Інше'
  );

  donate(): void {
    if (!this.request) return;
    if (this.amount() <= 0) { this.error.set('Введіть суму більше 0'); return; }

    this.isProcessing.set(true);
    this.error.set('');

    this.paymentService.donate({
      helpRequestId: this.request.id,
      amount:        this.amount(),
      comment:       this.comment() || `Донат SupportWay — ${this.request.title}`
    }).subscribe({
      next: res => {
        window.open(res.checkoutUrl, '_blank'); // Переходимо на Monobank
        this.isProcessing.set(false);
        this.closed.emit();
      },
      error: () => {
        this.error.set('Помилка при створенні платежу. Спробуйте ще раз.');
        this.isProcessing.set(false);
      }
    });
  }

  close(): void { this.closed.emit(); }
}
