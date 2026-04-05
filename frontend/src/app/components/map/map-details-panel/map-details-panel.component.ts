import { Component, input, output, computed, inject } from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe, SlicePipe } from '@angular/common';
import { HelpRequestDetails } from '../../../models/help-request.model';
import { MapMarkerDto, getTypeStyle } from '../../../models/map.models';
import { HelpRequestService } from '../../../services/help-request.service';

@Component({
  selector: 'app-map-details-panel',
  standalone: true,
  imports: [CommonModule, DecimalPipe, DatePipe, SlicePipe],
  templateUrl: './map-details-panel.component.html',
  styleUrls:   ['./map-details-panel.component.scss']
})
export class MapDetailsPanelComponent {
  readonly visible   = input.required<boolean>();
  readonly isLoading = input<boolean>(false);
  readonly error     = input<string | null>(null);
  readonly details   = input<HelpRequestDetails | null>(null);
  readonly marker    = input<MapMarkerDto | null>(null);

  readonly closed = output<void>();
  readonly retry  = output<void>();
  readonly donate = output<void>();

  // ─── Computed helpers ──────────────────────────────────────────────────────

  readonly accentColor = computed<string>(() => {
    const d = this.details();
    if (!d?.requestItems?.length) return '#2563eb';
    return getTypeStyle(d.requestItems[0].supportTypeName).color;
  });

  readonly progress = computed<number>(() => {
    const d = this.details();
    if (!d || d.targetAmount <= 0) return 0;
    return Math.min(100, Math.round((d.collectedAmount / d.targetAmount) * 100));
  });

  readonly firstChar = computed<string>(() =>
    (this.details()?.userName ?? '?').charAt(0).toUpperCase()
  );

  private readonly helpRequestService = inject(HelpRequestService);

  getTypeStyle = getTypeStyle;

  imageSrc(base64?: string | null): string | null {
    return this.helpRequestService.getImageSrc(base64);
  }

  // Tag color helpers
  tagBg(typeName: string): string {
    return getTypeStyle(typeName).colorLight;
  }
  tagColor(typeName: string): string {
    return getTypeStyle(typeName).colorText;
  }
}
