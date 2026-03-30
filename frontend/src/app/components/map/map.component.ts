import {
  Component, OnInit, OnDestroy, inject, signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { Subject, switchMap, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { MapDataService } from '../../services/map-data.service';
import { MapFilterComponent } from '../map-filter/map-filter.component';
import { DonateModalComponent } from '../donate-modal/donate-modal.component';
import {
  MapFilterParams, RequestMapDto, getPrimaryTypeStyle
} from '../../models/map.models';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, MapFilterComponent, DonateModalComponent],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.scss']
})
export class MapComponent implements OnInit, OnDestroy {
  private readonly mapDataService = inject(MapDataService);

  private map!: L.Map;
  private clusterGroup: any; // MarkerClusterGroup
  private destroy$ = new Subject<void>();
  private filterChange$ = new Subject<MapFilterParams>();

  readonly isLoading        = signal(false);
  readonly totalCount       = signal(0);
  readonly selectedRequest  = signal<RequestMapDto | null>(null);
  readonly showDonate       = signal(false);

  ngOnInit(): void {
    this.initMap();
    this.listenToFilterChanges();
    // Перший завантаження — без фільтрів
    this.filterChange$.next({});
  }

  // ── Ініціалізація карти ────────────────────────────────────────────────────

  private initMap(): void {
    this.map = L.map('map', {
      center: [49.0, 31.5], // Центр України
      zoom: 6,
      zoomControl: true,
      attributionControl: false
    });

    L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
      subdomains: 'abcd',
      maxZoom: 20
    }).addTo(this.map);

    // Кластеризація — без неї при 300+ маркерах браузер помітно гальмує
    this.clusterGroup = (L as any).markerClusterGroup({
      chunkedLoading: true,       // рендеримо маркери порціями, не блокуємо UI
      maxClusterRadius: 60,
      showCoverageOnHover: false,
      iconCreateFunction: (cluster: any) => L.divIcon({
        html: `<div class="sw-cluster">${cluster.getChildCount()}</div>`,
        className: '',
        iconSize: L.point(40, 40)
      })
    });
    this.map.addLayer(this.clusterGroup);
  }

  // ── Підписка на зміни фільтрів з RxJS оптимізацією ───────────────────────

  private listenToFilterChanges(): void {
    this.filterChange$.pipe(
      debounceTime(300),           // чекаємо 300ms після останньої зміни
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
      switchMap(filter => {        // скасовує попередній запит якщо прийшов новий
        this.isLoading.set(true);
        return this.mapDataService.getMarkers(filter);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: result => {
        this.renderMarkers(result.items);
        this.totalCount.set(result.total);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ── Рендер маркерів ────────────────────────────────────────────────────────

  private renderMarkers(requests: RequestMapDto[]): void {
    this.clusterGroup.clearLayers();

    requests.forEach(req => {
      const style   = getPrimaryTypeStyle(req.supportTypes);
      const percent = req.targetAmount > 0
        ? Math.min(100, Math.round((req.collectedAmount / req.targetAmount) * 100))
        : 0;

      const icon = L.divIcon({
        html: `
          <div class="sw-pin" style="--c:${style.color}">
            <span>${style.icon}</span>
            <div class="sw-pin-bar" style="width:${percent}%"></div>
          </div>`,
        className: '',
        iconSize:   [40, 48],
        iconAnchor: [20, 48],
        popupAnchor:[0, -52]
      });

      const marker = L.marker([req.latitude, req.longitude], { icon });
      marker.bindPopup(this.buildPopup(req, style, percent), { maxWidth: 260 });
      this.clusterGroup.addLayer(marker);
    });
  }

  private buildPopup(
    req: RequestMapDto,
    style: { color: string; icon: string },
    percent: number
  ): string {
    const typeLabels = req.supportTypes.map(t => t.nameOfType).join(', ') || 'Інше';
    const collected  = req.collectedAmount.toLocaleString('uk-UA');
    const target     = req.targetAmount.toLocaleString('uk-UA');

    return `
      <div class="sw-popup">
        <div class="sw-popup-type" style="color:${style.color}">
          ${style.icon} ${typeLabels}
        </div>
        <p class="sw-popup-title">${req.title}</p>
        <p class="sw-popup-region">📍 ${req.region}</p>
        <div class="sw-popup-track">
          <div class="sw-popup-fill" style="width:${percent}%;background:${style.color}"></div>
        </div>
        <div class="sw-popup-nums">
          <span>${collected} ₴</span>
          <span>${percent}% з ${target} ₴</span>
        </div>
        <button class="sw-popup-btn"
          onclick="window.dispatchEvent(new CustomEvent('sw:donate',{detail:'${req.id}'}))">
          💙 Задонатити
        </button>
      </div>`;
  }

  // ── Публічні методи для шаблону ───────────────────────────────────────────

  onFiltersChanged(filters: MapFilterParams): void {
    this.filterChange$.next(filters);
  }

  onDonateRequested(req: RequestMapDto): void {
    this.selectedRequest.set(req);
    this.showDonate.set(true);
  }

  closeDonate(): void {
    this.showDonate.set(false);
    this.selectedRequest.set(null);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.map?.remove();
  }
}
