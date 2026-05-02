import { Component, input, output, computed, inject } from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe, SlicePipe } from '@angular/common';
import { Router } from '@angular/router';
import { HelpRequestDetails } from '../../../models/help-request.model';
import { MapMarkerDto, getTypeStyle } from '../../../models/map.models';
import { HelpRequestService } from '../../../services/help-request.service';

@Component({
  selector: 'app-map-details-panel',
  standalone: true,
  imports: [CommonModule, DecimalPipe, DatePipe, SlicePipe],
  templateUrl: './map-details-panel.component.html',
  styleUrls: ['./map-details-panel.component.scss']
})
export class MapDetailsPanelComponent {
  private readonly router = inject(Router);
  private readonly helpRequestService = inject(HelpRequestService);

  readonly visible = input.required<boolean>();
  readonly isLoading = input<boolean>(false);
  readonly error = input<string | null>(null);
  readonly details = input<HelpRequestDetails | null>(null);
  readonly marker = input<MapMarkerDto | null>(null);

  readonly closed = output<void>();
  readonly retry = output<void>();
  readonly donate = output<void>();

  readonly firstChar = computed<string>(() =>
    (this.details()?.userName ?? '?').charAt(0).toUpperCase()
  );

  readonly accentColor = computed<string>(() => {
    const details = this.details();
    if (!details?.requestItems?.length) return '#2563eb';
    return getTypeStyle(details.requestItems[0].supportTypeName).color;
  });

  readonly progress = computed<number>(() => {
    const details = this.details();
    if (!details || details.targetAmount <= 0) return 0;
    return Math.min(100, Math.round((details.collectedAmount / details.targetAmount) * 100));
  });

  readonly formattedDistance = computed<string | null>(() => {
    const distanceKm = this.marker()?.distanceKm;
    if (distanceKm == null) return null;
    return this.formatDistance(distanceKm);
  });

  getTypeStyle = getTypeStyle;

  openProfile(): void {
    const name = this.details()?.userName;
    if (name) {
      this.router.navigate(['/profile', name]);
    }
  }

  imageSrc(base64?: string | null): string | null {
    return this.helpRequestService.getImageSrc(base64);
  }

  tagBg(typeName: string): string {
    return getTypeStyle(typeName).colorLight;
  }

  tagColor(typeName: string): string {
    return getTypeStyle(typeName).colorText;
  }

  private formatDistance(distanceKm: number): string {
    if (distanceKm < 1) {
      return `${Math.round(distanceKm * 1000)} м`;
    }

    return `${distanceKm.toFixed(1)} км`;
  }
}
