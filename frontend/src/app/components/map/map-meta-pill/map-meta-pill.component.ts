import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-map-meta-pill',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-meta-pill.component.html',
  styleUrls: ['./map-meta-pill.component.scss']
})
export class MapMetaPillComponent {
  readonly isLoading  = input.required<boolean>();
  readonly totalCount = input.required<number>();
}
