import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ToastType = 'info' | 'success' | 'badge';

export interface Toast {
  id: string;
  type: ToastType;
  title: string;
  message: string;
  imageBase64?: string | null;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toasts$ = new BehaviorSubject<Toast[]>([]);

  show(type: ToastType, title: string, message: string, imageBase64?: string | null): void {
    const toast: Toast = {
      id: crypto.randomUUID(),
      type,
      title,
      message,
      imageBase64
    };

    this.toasts$.next([...this.toasts$.getValue(), toast]);

    setTimeout(() => this.dismiss(toast.id), 4500);
  }

  dismiss(id: string): void {
    this.toasts$.next(this.toasts$.getValue().filter(t => t.id !== id));
  }
}
