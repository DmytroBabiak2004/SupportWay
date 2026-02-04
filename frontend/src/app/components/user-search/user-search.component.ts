// user-search.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { UserSearch, UserSearchService } from '../../services/user-search.service';
import { ChatService } from '../../services/chat.service';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';
import { NgForOf, NgIf } from '@angular/common';

@Component({
  selector: 'app-user-search',
  standalone: true,
  templateUrl: './user-search.component.html',
  imports: [FormsModule, NgForOf, NgIf],
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
    private router: Router
  ) {
    this.currentUserId = this.authService.getUserId();
  }

  searchUsers() {
    if (!this.searchText.trim()) {
      this.results = [];
      return;
    }

    this.isLoading = true;

    this.userSearchService.searchUsers(this.searchText).subscribe({
      next: users => {
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
    // Передаємо аргументи окремо: спочатку свій ID, потім ID знайденого користувача
    this.chatService.createChat(this.currentUserId, user.id).subscribe({
      next: (chat) => {
        console.log('Chat created or found:', chat);
        // Переходимо на сторінку чату і передаємо ID чату через queryParams
        this.router.navigate(['/home'], { queryParams: { chatId: chat.id } });
      },
      error: (err) => {
        console.error('Помилка при створенні чату:', err);
      }
    });
  }

}
