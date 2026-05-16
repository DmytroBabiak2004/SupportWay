import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { Toast, ToastService } from '../../../services/toast.service';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-container.component.html',
  styleUrls: ['./toast-container.component.scss']
})
export class ToastContainerComponent implements OnInit, OnDestroy {

  toasts: Toast[] = [];
  private sub = new Subscription();

  constructor(
    private toastService: ToastService,
    private notificationService: NotificationService
  ) {}

  trackById(index: number, item: Toast): string {
    return item.id;
  }

  ngOnInit(): void {
    this.sub.add(
      this.toastService.toasts$.subscribe(t => this.toasts = t)
    );
  }

  getImageSrc(base64?: string | null): string | null {
    return this.notificationService.getImageSrc(base64);
  }

  dismiss(id: string): void {
    this.toastService.dismiss(id);
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}
