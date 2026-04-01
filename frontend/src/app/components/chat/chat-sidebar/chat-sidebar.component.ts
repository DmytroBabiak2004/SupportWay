import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, HostListener, HostBinding } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../services/chat.service';
import { ChatListItem } from '../../../models/chat.model';
import { ProfileService } from '../../../services/profile.service';

@Component({
  selector: 'app-chat-sidebar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-sidebar.component.html',
  styleUrls: ['./chat-sidebar.component.scss'],
})
export class ChatSidebarComponent implements OnChanges {
  @Input() chats: ChatListItem[] = [];
  @Input() selectedChatId: string | null = null;
  @Input() isOpen = true;

  @Output() chatSelected = new EventEmitter<ChatListItem>();
  @Output() chatDeleted  = new EventEmitter<string>();

  searchText = '';
  filteredChats: ChatListItem[] = [];

  // --- Логіка зміни ширини сайдбару ---
  // Робимо null, щоб за замовчуванням (наприклад, на мобілці) інлайн-стиль не застосовувався,
  // а працював CSS (flex або width: 100%).
  @HostBinding('style.width.px') sidebarWidth: number | null = 320;
  isResizing = false;

  constructor(
    private chatService: ChatService,
    public profileService: ProfileService
  ) {}

  ngOnInit() {
    this.checkScreenSize();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['chats']) this.filterChats();
  }

  filterChats(): void {
    const term = this.searchText.trim().toLowerCase();
    this.filteredChats = term
      ? this.chats.filter(c => c.displayName.toLowerCase().includes(term))
      : [...this.chats];
  }

  openChat(chat: ChatListItem): void { this.chatSelected.emit(chat); }

  deleteChat(chat: ChatListItem, event: Event): void {
    event.stopPropagation();
    if (!confirm(`Видалити чат із "${chat.displayName}"?`)) return;
    this.chatService.deleteChat(chat.id).subscribe({
      next: () => this.chatDeleted.emit(chat.id),
      error: console.error
    });
  }

  getInitials(name: string): string {
    return name?.split(' ').slice(0, 2).map(n => n[0]?.toUpperCase()).join('') || '?';
  }

  formatTime(iso?: string): string {
    if (!iso) return '';
    const d = new Date(iso);
    const now = new Date();
    if (d.toDateString() === now.toDateString())
      return d.toLocaleTimeString('uk-UA', { hour: '2-digit', minute: '2-digit' });
    const yesterday = new Date(now); yesterday.setDate(now.getDate() - 1);
    if (d.toDateString() === yesterday.toDateString()) return 'Вчора';
    return d.toLocaleDateString('uk-UA', { day: '2-digit', month: '2-digit' });
  }

  // --- Відстеження розміру екрану ---
  @HostListener('window:resize', ['$event'])
  onResize() {
    this.checkScreenSize();
  }

  private checkScreenSize() {
    // Якщо екран мобільний, знімаємо інлайн-ширину, щоб працював CSS
    if (window.innerWidth <= 768) {
      this.sidebarWidth = null;
    } else {
      // Якщо повертаємось на десктоп і ширина не встановлена, ставимо базову
      if (this.sidebarWidth === null) {
        this.sidebarWidth = 320;
      }
    }
  }

  // --- Методи для перетягування (Resize) ---
  startResize(event: MouseEvent): void {
    // Забороняємо ресайз на мобільних
    if (window.innerWidth <= 768) return;

    this.isResizing = true;
    event.preventDefault();
  }

  @HostListener('document:mousemove', ['$event'])
  onMouseMove(event: MouseEvent): void {
    if (!this.isResizing) return;

    let newWidth = event.clientX;

    if (newWidth < 250) newWidth = 250;
    if (newWidth > 500) newWidth = 500;

    this.sidebarWidth = newWidth;
  }

  @HostListener('document:mouseup')
  onMouseUp(): void {
    this.isResizing = false;
  }
}
