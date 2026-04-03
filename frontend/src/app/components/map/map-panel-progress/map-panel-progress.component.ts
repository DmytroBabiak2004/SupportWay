import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-map-panel-progress',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-panel-progress.component.html',
  styleUrls: ['./map-panel-progress.component.scss']
})
export class MapPanelProgressComponent {
  readonly collectedAmount = input.required<number>();
  readonly targetAmount    = input.required<number>();

  readonly percent = computed(() => {
    if (this.targetAmount() <= 0) return 0;
    return Math.min(100, Math.round((this.collectedAmount() / this.targetAmount()) * 100));
  });

  protected readonly Math = Math;
}
