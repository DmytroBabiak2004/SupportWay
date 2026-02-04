import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // <--- 1. Важливо: імпорт для ngModel
import { ChatService } from '../../../services/chat.service'; // Перевірте шлях до моделі Chat

@Component({
  selector: 'app-chat-sidebar',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule // <--- 2. Додаємо сюди
  ],
  templateUrl: './chat-sidebar.component.html',
  styleUrls: ['./chat-sidebar.component.scss'],
})
export class ChatSidebarComponent implements OnChanges { // <--- 3. Додаємо OnChanges
  @Input() chats: any[] = [];
  @Input() selectedChat: any | null = null;
  @Input() isOpen = true;

  @Output() chatSelected = new EventEmitter<any>();
  @Output() chatDeleted = new EventEmitter<string>();

  // Змінні для пошуку
  searchText: string = '';
  filteredChats: any[] = []; // Саме цей масив ми будемо показувати в HTML

  constructor(private chatService: ChatService) {}

  // Цей метод спрацьовує, коли батьківський компонент оновлює список chats
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['chats']) {
      this.filterChats(); // Оновлюємо фільтрацію, якщо прийшли нові дані
    }
  }

  getChatName(chat: any): string {
    return chat.name || 'Unknown';
  }

  // Логіка пошуку
  filterChats(): void {
    if (!this.searchText.trim()) {
      // Якщо пошук пустий — показуємо всі чати
      this.filteredChats = this.chats;
    } else {
      // Інакше фільтруємо за назвою (без урахування регістру)
      const term = this.searchText.toLowerCase();
      this.filteredChats = this.chats.filter(chat =>
        this.getChatName(chat).toLowerCase().includes(term)
      );
    }
  }

  openChat(chat: any): void {
    this.chatSelected.emit(chat);
  }

  deleteChat(chat: any, event: Event): void {
    event.stopPropagation();
    const chatName = this.getChatName(chat);
    if (!confirm(`Видалити чат "${chatName}"?`)) return;

    this.chatService.deleteChat(chat.id).subscribe({
      next: () => this.chatDeleted.emit(chat.id),
      error: (err) => console.error(err),
    });
  }
}
