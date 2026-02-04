import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chat-composer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-composer.component.html',
  styleUrls: ['./chat-composer.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatComposerComponent {
  @Input() value = '';
  @Input() placeholder = 'Написати повідомлення...';
  @Input() disabled = false;

  @Output() valueChange = new EventEmitter<string>();
  @Output() send = new EventEmitter<void>();
  @Output() typing = new EventEmitter<void>();

  // Гетер, щоб перевірити, чи поле пусте (ігноруючи пробіли)
  get isSendDisabled(): boolean {
    return this.disabled || !this.value?.trim();
  }

  onInput(v: string): void {
    this.valueChange.emit(v);
    this.typing.emit();
  }

  onEnter(): void {
    if (this.isSendDisabled) return;
    this.send.emit();
  }

  onSendClick(): void {
    if (this.isSendDisabled) return;
    this.send.emit();
  }
}
