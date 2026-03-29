import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ProfileAnalyticsService } from '../../../services/profile-analytics.service';
import { ProfileAnalytics } from '../../../models/profile-analytics.model';

interface HeatmapDay {
  date: Date;
  dateString: string;
  count: number;
  level: number;
  tooltip: string; // <-- Заздалегідь згенерований текст для продуктивності
}

interface HeatmapMonth {
  name: string;
  col: number; // <-- Номер колонки (тижня), над якою має стояти місяць
}

@Component({
  selector: 'app-profile-heatmap',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-heatmap.component.html',
  styleUrls: ['./profile-heatmap.component.scss']
})
export class ProfileHeatmapComponent implements OnChanges {
  @Input({ required: true }) profileId!: string;

  loading = false;
  error = '';

  days: HeatmapDay[] = [];
  months: HeatmapMonth[] = []; // <-- Оновлений тип
  totalActivity = 0;

  readonly weekdays = ['Пн', 'Ср', 'Пт'];

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
        this.processData(data);
        this.loading = false;
      },
      error: (err) => {
        console.error('Heatmap loading error:', err);
        this.error = 'Не вдалося завантажити активність.';
        this.loading = false;
      }
    });
  }

  private processData(data: ProfileAnalytics): void {
    const activityMap = new Map<string, number>();

    const addToMap = (items: { date: string; count: number }[]) => {
      if (!items) return;
      items.forEach(item => {
        const dateKey = item.date.split('T')[0];
        const currentCount = activityMap.get(dateKey) || 0;
        activityMap.set(dateKey, currentCount + item.count);
      });
    };

    addToMap(data.postsByDate);
    addToMap(data.requestsActivity);

    this.totalActivity = Array.from(activityMap.values()).reduce((a, b) => a + b, 0);

    this.days = [];
    this.months = [];

    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(endDate.getDate() - 364);

    // Відкочуємося до понеділка
    while (startDate.getDay() !== 1) {
      startDate.setDate(startDate.getDate() - 1);
    }

    let currentMonth = -1;
    let iterDate = new Date(startDate);
    let daysCount = 0;

    while (iterDate <= endDate) {
      const dateKey = iterDate.toISOString().split('T')[0];
      const count = activityMap.get(dateKey) || 0;

      let level = 0;
      if (count > 0 && count <= 2) level = 1;
      else if (count > 2 && count <= 5) level = 2;
      else if (count > 5 && count <= 9) level = 3;
      else if (count > 9) level = 4;

      // Оптимізація: Генеруємо тултип один раз тут, а не в HTML
      const formattedDate = iterDate.toLocaleDateString('uk-UA', {
        day: 'numeric', month: 'long', year: 'numeric'
      });
      const tooltip = count === 0
        ? `Немає активності ${formattedDate}`
        : `${count} дій ${formattedDate}`;

      this.days.push({
        date: new Date(iterDate),
        dateString: dateKey,
        count,
        level,
        tooltip
      });

      // --- ЛОГІКА МІСЯЦІВ ---
      // Кожні 7 днів створюється нова колонка в CSS Grid.
      // Ми обчислюємо поточний індекс колонки (починаючи з 1)
      const col = Math.floor(daysCount / 7) + 1;

      if (iterDate.getMonth() !== currentMonth) {
        let monthName = iterDate.toLocaleString('uk-UA', { month: 'short' });
        // Робимо з великої літери
        monthName = monthName.charAt(0).toUpperCase() + monthName.slice(1);

        this.months.push({ name: monthName, col: col });
        currentMonth = iterDate.getMonth();
      }

      daysCount++;
      iterDate.setDate(iterDate.getDate() + 1);
    }
  }
}
