import { Component, ElementRef, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { UserSearch, UserSearchService } from '../../services/user-search.service';
import { ChatService } from '../../services/chat.service';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';
import { NgForOf, NgIf, UpperCasePipe } from '@angular/common';

@Component({
  selector: 'app-user-search',
  standalone: true,
  templateUrl: './user-search.component.html',
  imports: [FormsModule, NgForOf, NgIf, UpperCasePipe], // Додали UpperCasePipe для ініціалів
  styleUrls: ['./user-search.component.scss']
})
export class UserSearchComponent {

  searchText = '';
  results: UserSearch[] = [];
  isLoading = false;
  currentUserId: string;

  constructor(
    private userSearchService: UserSearchService,
    private chatService: ChatService,
    private authService: AuthService,
    private router: Router,
    private eRef: ElementRef // Для кліку зовні
  ) {
    this.currentUserId = this.authService.getUserId();
  }

  // Закриваємо дропдаун, якщо клікнули повз нього
  @HostListener('document:click', ['$event'])
  clickout(event: Event) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.clearSearch();
    }
  }

  searchUsers() {
    if (!this.searchText.trim()) {
      this.results = [];
      return;
    }

    this.isLoading = true;

    // Можна додати debounce (затримку), щоб не спамити запитами
    this.userSearchService.searchUsers(this.searchText).subscribe({
      next: users => {
        // Фільтруємо себе зі списку
        this.results = users.filter(u => u.id !== this.currentUserId);
        this.isLoading = false;
      },
      error: err => {
        console.error('Search error:', err);
        this.isLoading = false;
      }
    });
  }

  createChatWith(user: UserSearch): void {
    this.chatService.createChat(this.currentUserId, user.id).subscribe({
      next: (chat) => {
        this.router.navigate(['/chat'], { queryParams: { chatId: chat.id } });
        this.clearSearch(); // Очищуємо пошук після переходу
      },
      error: (err) => console.error('Error creating chat:', err)
    });
  }

  clearSearch() {
    this.searchText = '';
    this.results = [];
    this.isLoading = false;
  }
}
