import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chat } from '../../../services/chat.service';

@Component({
  selector: 'app-chat-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './chat-header.component.html',
  styleUrls: ['./chat-header.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChatHeaderComponent {
  @Input({ required: true }) chat!: Chat;
  @Input({ required: true }) title = '';
  @Input() typingText: string | null = null;

  @Output() back = new EventEmitter<void>();
  @Output() deleteChat = new EventEmitter<void>(); // Нова подія

  isMenuOpen = false; // Стан меню

  get initial(): string {
    return (this.title?.trim()?.[0] ?? '?').toUpperCase();
  }

  toggleMenu(event?: Event) {
    if (event) event.stopPropagation();
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu() {
    this.isMenuOpen = false;
  }

  onDeleteClick() {
    this.deleteChat.emit();
    this.closeMenu();
  }
}
