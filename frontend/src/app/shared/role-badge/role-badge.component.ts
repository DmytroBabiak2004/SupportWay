import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

export type RoleBadgeType = 1 | 2 | 3 | number | null | undefined;

@Component({
  selector: 'app-role-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './role-badge.component.html',
  styleUrls: ['./role-badge.component.scss']
})
export class RoleBadgeComponent {
  @Input() isVerified: boolean | null | undefined = false;
  @Input() verifiedAs: RoleBadgeType = null;
  @Input() size: 'xs' | 'sm' | 'md' = 'sm';

  get shouldShow(): boolean {
    return !!this.isVerified && (this.verifiedAs === 1 || this.verifiedAs === 2);
  }

  get isVolunteer(): boolean {
    return this.verifiedAs === 1;
  }

  get isMilitary(): boolean {
    return this.verifiedAs === 2;
  }

  get label(): string {
    if (this.isVolunteer) return 'Підтверджений волонтер';
    if (this.isMilitary) return 'Підтверджений військовий';
    return 'Підтверджений профіль';
  }
}
