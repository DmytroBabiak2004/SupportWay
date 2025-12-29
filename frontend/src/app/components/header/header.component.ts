import {Component} from '@angular/core';
import {RouterModule} from '@angular/router';
import {NgClass, NgIf,} from '@angular/common';
import { AuthService } from '../../services/auth.service';
import {UserSearchComponent} from '../user-search/user-search.component';


@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  standalone: true,
  imports: [RouterModule, NgClass, UserSearchComponent],
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {
  isMenuOpen: boolean = false;


  constructor(public authService: AuthService) {}

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  closeMenu(): void {
    this.isMenuOpen = false;
  }

  handleNavClick(): void {
    // Закриваємо меню лише якщо ширина екрану менша за 770px
    if (window.innerWidth <= 770) {
      this.closeMenu();
    }
  }

  onLogout() {
    this.authService.logout();
  }
  dropdownOpen = false;

  toggleDropdown() {
    this.dropdownOpen = !this.dropdownOpen;
  }


}
