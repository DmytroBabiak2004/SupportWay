import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common'; // DatePipe вже всередині
import { FormsModule } from '@angular/forms';
import { Profile } from '../../../models/profile.model';
import { ProfileService } from '../../../services/profile.service';

@Component({
  selector: 'app-profile-card',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile-card.component.html',
  styleUrls: ['./profile-card.component.scss']
})
export class ProfileCardComponent {

  @Input({ required: true }) profile!: Profile;
  @Input() loading = false;
  @Input() isOwnProfile = false;
  @Input() isFollowing = false;
  @Input() uploadingPhoto = false;
  @Input() savingDescription = false;

  @Output() photoSelected = new EventEmitter<Event>();
  @Output() rateProfile = new EventEmitter<number>();
  @Output() descriptionSaved = new EventEmitter<string>();
  @Output() followToggled = new EventEmitter<void>();
  @Output() messageSent = new EventEmitter<void>();
  @Output() loggedOut = new EventEmitter<void>();
  @Output() settingsNavigated = new EventEmitter<void>();

  isEditingDescription = false;
  descriptionDraft = '';

  /** Чи розгорнуто повний опис профілю */
  bioExpanded = false;

  /** Чи відкрите dropdown-меню налаштувань */
  settingsOpen = false;

  constructor(public profileService: ProfileService) {}

  get initials(): string {
    if (!this.profile) return '?';
    const first = this.profile.name?.trim()[0] || '';
    const last = this.profile.fullName?.trim()[0] || '';
    if (first || last) return (first + last).toUpperCase();
    return (this.profile.username?.[0] || '?').toUpperCase();
  }

  // --- Управління описом ---

  startEditDescription(): void {
    this.descriptionDraft = this.profile?.description ?? '';
    this.isEditingDescription = true;
    this.bioExpanded = false; // скидаємо розгортку при переході в режим редагування
  }

  cancelEditDescription(): void {
    this.isEditingDescription = false;
    this.descriptionDraft = '';
  }

  saveDescription(): void {
    if (this.descriptionDraft !== this.profile.description) {
      this.descriptionSaved.emit(this.descriptionDraft);
    }
    this.isEditingDescription = false;
  }

  // --- Налаштування ---

  /**
   * Перемикає dropdown налаштувань.
   * Закривається автоматично при кліку поза компонентом —
   * рекомендовано додати (clickOutside) або HostListener у батьківському компоненті.
   */
  toggleSettings(): void {
    this.settingsOpen = !this.settingsOpen;
  }

  navigateToSettings(): void {
    this.settingsOpen = false;
    this.settingsNavigated.emit();
  }

  // --- Методи-обгортки для подій з HTML ---

  onPhotoSelected(event: Event): void {
    this.photoSelected.emit(event);
  }

  setRate(star: number): void {
    if (this.isOwnProfile) return;
    this.rateProfile.emit(star);
  }

  toggleFollow(): void {
    this.followToggled.emit();
  }

  sendMessage(): void {
    this.messageSent.emit();
  }

  onLogout(): void {
    this.settingsOpen = false;
    this.loggedOut.emit();
  }
}
