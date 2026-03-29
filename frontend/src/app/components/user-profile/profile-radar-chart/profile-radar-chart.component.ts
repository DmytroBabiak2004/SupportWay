import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ProfileAnalyticsService } from '../../../services/profile-analytics.service'; // Перевір шлях до сервісу
import { AnalyticsTypeItem } from '../../../models/profile-analytics.model'; // Перевір шлях до моделі

interface RadarAxis {
  id: string;
  label: string;
  value: number;
}

@Component({
  selector: 'app-profile-radar-chart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-radar-chart.component.html',
  styleUrls: ['./profile-radar-chart.component.scss']
})
export class ProfileRadarChartComponent implements OnChanges {
  @Input({ required: true }) profileId!: string;

  requestsByType: AnalyticsTypeItem[] = [];
  loading = false;
  error = '';

  readonly size = 460;
  readonly center = this.size / 2;
  readonly radius = 130;
  readonly levels = 4;

  // Залишаємо базові осі, щоб діаграма завжди була п'ятикутником
  readonly supportTypes = [
    { id: '0013977c-4d16-4ec4-8e81-7906ad21c6c9', label: 'Медична допомога' },
    { id: '08ba535a-5e7f-4a63-be2f-5d9cd29b6cee', label: 'Гуманітарна допомога' },
    { id: '75fb265c-737c-49c2-959f-7ae11b30c7f5', label: 'Логістика' },
    { id: 'daf6f6d6-437f-45fb-a979-55d506b04fee', label: 'Дрони та БПЛА' },
    { id: 'fdb98e31-1283-4761-9d13-302f6a59f963', label: 'Евакуація' }
  ];

  constructor(private analyticsService: ProfileAnalyticsService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['profileId']?.currentValue) {
      this.loadData();
    }
  }

  private loadData(): void {
    if (!this.profileId?.trim()) return;

    this.loading = true;
    this.error = '';

    this.analyticsService.getDashboard(this.profileId).subscribe({
      next: (data) => {
        // Беремо масив запитів з бекенду. Якщо його немає - ставимо пустий масив.
        this.requestsByType = data.requestsByType || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Radar chart loading error:', err);
        this.error = 'Не вдалося завантажити аналітику.';
        this.loading = false;
      }
    });
  }

  // --- ЛОГІКА ПОБУДОВИ ДІАГРАМИ ---

  get normalizedData(): RadarAxis[] {
    return this.supportTypes.map(type => {
      // Шукаємо, чи є для цієї осі дані з бекенду
      const found = this.requestsByType.find(x => x.typeId === type.id);
      return {
        id: type.id,
        label: type.label,
        value: found?.totalQuantity ?? 0
      };
    });
  }

  get maxValue(): number {
    const max = Math.max(...this.normalizedData.map(x => x.value), 0);
    return max > 0 ? max : 1;
  }

  get totalQuantity(): number {
    return this.normalizedData.reduce((sum, item) => sum + item.value, 0);
  }

  get dominantType(): RadarAxis | null {
    const sorted = [...this.normalizedData].sort((a, b) => b.value - a.value);
    return sorted[0]?.value > 0 ? sorted[0] : null;
  }

  get chartPolygon(): string {
    return this.normalizedData
      .map((item, index) => {
        const ratio = item.value / this.maxValue;
        const point = this.getPoint(index, ratio);
        return `${point.x},${point.y}`;
      })
      .join(' ');
  }

  get axisLines(): { x1: number; y1: number; x2: number; y2: number }[] {
    return this.normalizedData.map((_, index) => {
      const outer = this.getPoint(index, 1);
      return { x1: this.center, y1: this.center, x2: outer.x, y2: outer.y };
    });
  }

  get levelPolygons(): string[] {
    return Array.from({ length: this.levels }, (_, i) => {
      const ratio = (i + 1) / this.levels;
      return this.normalizedData
        .map((_, index) => {
          const point = this.getPoint(index, ratio);
          return `${point.x},${point.y}`;
        })
        .join(' ');
    });
  }

  get dataPoints(): { x: number; y: number; value: number; label: string }[] {
    return this.normalizedData.map((item, index) => {
      const ratio = item.value / this.maxValue;
      const point = this.getPoint(index, ratio);
      return { x: point.x, y: point.y, value: item.value, label: item.label };
    });
  }

  get labels(): { x: number; y: number; textAnchor: string; label: string; value: number }[] {
    return this.normalizedData.map((item, index) => {
      const point = this.getPoint(index, 1.35);

      let textAnchor: 'start' | 'middle' | 'end' = 'middle';
      if (point.x < this.center - 20) textAnchor = 'end';
      if (point.x > this.center + 20) textAnchor = 'start';

      return { x: point.x, y: point.y, textAnchor, label: item.label, value: item.value };
    });
  }

  trackById(_: number, item: { id: string }): string {
    return item.id;
  }

  private getPoint(index: number, ratio: number): { x: number; y: number } {
    const total = this.normalizedData.length;
    const angle = (-Math.PI / 2) + (2 * Math.PI * index / total);

    return {
      x: this.center + Math.cos(angle) * this.radius * ratio,
      y: this.center + Math.sin(angle) * this.radius * ratio
    };
  }
}
