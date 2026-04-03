import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-map-panel-actions',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-panel-actions.component.html',
  styleUrls: ['./map-panel-actions.component.scss']
})
export class MapPanelActionsComponent {
  readonly isActive      = input.required<boolean>();
  readonly likesCount    = input<number>(0);
  readonly commentsCount = input<number>(0);

  readonly donate = output<void>();
}
