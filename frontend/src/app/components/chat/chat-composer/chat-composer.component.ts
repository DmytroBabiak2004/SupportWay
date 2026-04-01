import { Component, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat-composer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-composer.component.html',
  styleUrls: ['./chat-composer.component.scss']
})
export class ChatComposerComponent {
  @Output() messageSent = new EventEmitter<string>();
  @Output() typing = new EventEmitter<void>();

  // Отримуємо доступ до textarea через шаблонну змінну #messageInput
  @ViewChild('messageInput') messageInput!: ElementRef<HTMLTextAreaElement>;

  text = '';

  send(): void {
    const t = this.text.trim();
    if (!t) return;

    this.messageSent.emit(t);
    this.text = '';

    // Скидаємо висоту поля після відправки повідомлення.
    // Використовуємо setTimeout, щоб Angular встиг оновити DOM (очистити поле)
    setTimeout(() => this.resizeTextarea(), 0);
  }

  onInput(): void {
    this.typing.emit();
    this.resizeTextarea();
  }

  onKeydown(e: KeyboardEvent): void {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault(); // Запобігаємо переносу рядка
      this.send();
    }
  }

  // Приватний метод для логіки авторозширення
  private resizeTextarea(): void {
    if (!this.messageInput) return;

    const textarea = this.messageInput.nativeElement;
    // Спочатку скидаємо висоту, щоб правильно вирахувати нову
    textarea.style.height = 'auto';
    // Встановлюємо висоту відповідно до реального контенту
    textarea.style.height = `${textarea.scrollHeight}px`;
  }
}
