import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-map-side-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-side-panel.component.html',
  styleUrls: ['./map-side-panel.component.scss']
})
export class MapSidePanelComponent {
  readonly visible   = input.required<boolean>();
  readonly isLoading = input<boolean>(false);
  readonly error     = input<string | null>(null);

  readonly closed = output<void>();
  readonly retry  = output<void>();
}
