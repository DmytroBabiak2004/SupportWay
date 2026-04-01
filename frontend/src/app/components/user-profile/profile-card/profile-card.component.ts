import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Profile } from '../../../models/profile.model';
import { ProfileService } from '../../../services/profile.service';
import { VERIFICATION_ICONS, VERIFICATION_COLORS, VERIFICATION_LABELS } from '../../../models/verification.model';

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

  @Output() photoSelected     = new EventEmitter<Event>();
  @Output() rateProfile       = new EventEmitter<number>();
  @Output() descriptionSaved  = new EventEmitter<string>();
  @Output() followToggled     = new EventEmitter<void>();
  @Output() messageSent       = new EventEmitter<void>();
  @Output() loggedOut         = new EventEmitter<void>();
  @Output() followersClicked  = new EventEmitter<void>();
  @Output() followingClicked  = new EventEmitter<void>();
  @Output() verifyRequested   = new EventEmitter<void>();

  isEditingDescription = false;
  descriptionDraft = '';
  bioExpanded = false;
  settingsOpen = false;

  readonly ICONS   = VERIFICATION_ICONS;
  readonly COLORS  = VERIFICATION_COLORS;
  readonly LABELS  = VERIFICATION_LABELS;

  constructor(public profileService: ProfileService) {}

  get initials(): string {
    if (!this.profile) return '?';
    const f = this.profile.name?.trim()[0] || '';
    const l = this.profile.fullName?.trim()[0] || '';
    if (f || l) return (f + l).toUpperCase();
    return (this.profile.username?.[0] || '?').toUpperCase();
  }

  get verificationIcon(): string {
    return this.profile?.isVerified && this.profile.verifiedAs
      ? this.ICONS[this.profile.verifiedAs] ?? ''
      : '';
  }

  get verificationColor(): string {
    return this.profile?.isVerified && this.profile.verifiedAs
      ? this.COLORS[this.profile.verifiedAs] ?? ''
      : '';
  }

  get verificationLabel(): string {
    return this.profile?.isVerified && this.profile.verifiedAs
      ? this.LABELS[this.profile.verifiedAs] ?? ''
      : '';
  }

  startEditDescription(): void {
    this.descriptionDraft      = this.profile?.description ?? '';
    this.isEditingDescription  = true;
    this.bioExpanded           = false;
  }

  cancelEditDescription(): void {
    this.isEditingDescription = false;
    this.descriptionDraft = '';
  }

  saveDescription(): void {
    if (this.descriptionDraft !== this.profile.description)
      this.descriptionSaved.emit(this.descriptionDraft);
    this.isEditingDescription = false;
  }

  toggleSettings(): void { this.settingsOpen = !this.settingsOpen; }
  onPhotoSelected(event: Event): void { this.photoSelected.emit(event); }
  setRate(star: number): void { if (!this.isOwnProfile) this.rateProfile.emit(star); }
  toggleFollow(): void { this.followToggled.emit(); }
  sendMessage(): void { this.messageSent.emit(); }
  onLogout(): void { this.settingsOpen = false; this.loggedOut.emit(); }
  requestVerify(): void { this.settingsOpen = false; this.verifyRequested.emit(); }
}
