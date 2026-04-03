import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HelpRequestItemDetails } from '../../../models/help-request.model';

const TYPE_ICONS: Record<string, string> = {
  'Медицина':       '💊',
  'Харчування':     '🍞',
  'Одяг':           '👕',
  'Техніка':        '💻',
  'Транспорт':      '🚗',
  'Будівництво':    '🔧',
  'Зброя':          '🛡️',
  'Звязок':       '📡',
};

@Component({
  selector: 'app-map-panel-items',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-panel-items.component.html',
  styleUrls: ['./map-panel-items.component.scss']
})
export class MapPanelItemsComponent {
  readonly items = input<HelpRequestItemDetails[]>([]);

  getIcon(typeName: string): string {
    return TYPE_ICONS[typeName] ?? '📦';
  }
}
