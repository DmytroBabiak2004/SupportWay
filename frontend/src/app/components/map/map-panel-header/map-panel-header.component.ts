import { Component, input, computed } from '@angular/core';
import { CommonModule, UpperCasePipe } from '@angular/common';
import { HelpRequestItemDetails } from '../../../models/help-request.model';

@Component({
  selector: 'app-map-panel-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-panel-header.component.html',
  styleUrls: ['./map-panel-header.component.scss']
})
export class MapPanelHeaderComponent {
  readonly imageSrc        = input<string | null>(null);
  readonly authorPhotoSrc  = input<string | null>(null);
  readonly title           = input.required<string>();
  readonly userName        = input.required<string>();
  readonly createdAt       = input.required<string>();
  readonly requestItems    = input<HelpRequestItemDetails[]>([]);
  readonly locationName    = input<string | null>(null);
  readonly locationAddress = input<string | null>(null);

  readonly firstChar = computed(() =>
    this.userName().charAt(0).toUpperCase()
  );
}
