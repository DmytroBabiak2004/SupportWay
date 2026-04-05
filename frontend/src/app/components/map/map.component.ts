import {
  Component, OnInit, OnDestroy, inject, signal, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, switchMap, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { MapDataService }       from '../../services/map-data.service';
import { HelpRequestService }   from '../../services/help-request.service';
import { MapFilterComponent }   from './map-filter/map-filter.component';
import { DonateModalComponent } from '../../dialog/donate-modal/donate-modal.component';
import { MapDetailsPanelComponent } from './map-details-panel/map-details-panel.component';

import {
  MapFilterParams, MapMarkerDto, PagedResult, getTypeStyle
} from '../../models/map.models';
import { HelpRequestDetails } from '../../models/help-request.model';

declare const L: any;

// ─── Pin size calculation ─────────────────────────────────────────────────────
//
// Розмір піну залежить від targetAmount збору.
// Логарифмічна шкала: різниця відчутна, але великі суми не роблять велетенський пін.
//
// Межі:
//   PIN_MIN_SIZE  — мінімум (маленький збір або targetAmount = 0)
//   PIN_MAX_SIZE  — максимум (будь-яка сума вище AMOUNT_MAX не збільшує пін далі)
//   AMOUNT_MIN    — суми нижче цієї = мінімальний пін
//   AMOUNT_MAX    — суми вище цієї = максимальний пін

const PIN_MIN_SIZE  = 28;   // px (ширина viewBox-юніт-base)
const PIN_MAX_SIZE  = 52;   // px
const AMOUNT_MIN    = 1_000;
const AMOUNT_MAX    = 200_000;

function calcPinSize(targetAmount: number): number {
  if (!targetAmount || targetAmount <= 0) return PIN_MIN_SIZE;
  if (targetAmount <= AMOUNT_MIN)         return PIN_MIN_SIZE;
  if (targetAmount >= AMOUNT_MAX)         return PIN_MAX_SIZE;

  // Логарифмічна інтерполяція — сприйняття суми логарифмічне
  const logMin = Math.log(AMOUNT_MIN);
  const logMax = Math.log(AMOUNT_MAX);
  const logVal = Math.log(targetAmount);

  const t = (logVal - logMin) / (logMax - logMin); // 0..1
  return Math.round(PIN_MIN_SIZE + t * (PIN_MAX_SIZE - PIN_MIN_SIZE));
}

// ─── Build SVG pin ────────────────────────────────────────────────────────────
//
// ВАЖЛИВО: жодних <defs id="..."> — всі id глобальні в DOM,
// тому кілька пінів з однаковим id ламають відображення.
// Тінь винесена в CSS на .sw-pin-wrapper (drop-shadow на wrapper, не на SVG).

function buildPinSvg(color: string, progress: number, size: number): string {
  const pct = Math.max(0, Math.min(100, progress));

  // viewBox завжди 36×50, але rendered size задається через iconSize
  // Радіус прогрес-арки масштабуємо разом із піном
  const scale = size / 36;           // відношення rendered px до viewBox width
  const r     = Math.round(8 * scale);
  const circ  = 2 * Math.PI * r;
  const dash  = (pct / 100) * circ;
  const offset = (circ * 0.25).toFixed(2);
  // cx/cy/r для inner circle і dot також масштабуємо
  const cx = 18, cy = 16;
  const innerR = Math.round(9.5 * scale);
  const dotR   = Math.round(3.5 * scale);

  return `<svg class="sw-pin-svg" viewBox="0 0 36 50" xmlns="http://www.w3.org/2000/svg">
  <path d="M18 2C10.268 2 4 8.268 4 16C4 26 18 48 18 48C18 48 32 26 32 16C32 8.268 25.732 2 18 2Z" fill="${color}"/>
  <circle cx="${cx}" cy="${cy}" r="${innerR}" fill="rgba(255,255,255,0.96)"/>
  <circle cx="${cx}" cy="${cy}" r="${r}" fill="none" stroke="${color}" stroke-width="2"
          stroke-dasharray="${dash.toFixed(2)} ${circ.toFixed(2)}"
          stroke-dashoffset="${offset}"
          stroke-linecap="round" opacity="0.75"/>
  <circle cx="${cx}" cy="${cy}" r="${dotR}" fill="${color}"/>
</svg>`.trim();
}

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [
    CommonModule,
    MapFilterComponent,
    DonateModalComponent,
    MapDetailsPanelComponent,
  ],
  templateUrl: './map.component.html',
  styleUrls:   ['./map.component.scss']
})
export class MapComponent implements OnInit, OnDestroy {

  private readonly mapDataService     = inject(MapDataService);
  private readonly helpRequestService = inject(HelpRequestService);
  private readonly cdr                = inject(ChangeDetectorRef);

  private map!: any;
  private clusterGroup: any;
  private activeLeafletMarker: any    = null;
  private destroy$      = new Subject<void>();
  private filterChange$ = new Subject<MapFilterParams>();
  private lastMarker: MapMarkerDto | null = null;
  private allMarkers: MapMarkerDto[]      = [];

  readonly isLoading       = signal(false);
  readonly isDetailLoading = signal(false);
  readonly totalCount      = signal(0);

  readonly selectedMarker  = signal<MapMarkerDto | null>(null);
  readonly selectedDetails = signal<HelpRequestDetails | null>(null);
  readonly detailsError    = signal<string | null>(null);
  readonly showDonate      = signal(false);

  // ─────────────────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.initMap();
    this.listenToGlobalDonateEvent();
    this.listenToFilterChanges();
    this.filterChange$.next({});
  }

  // ─── Map init ────────────────────────────────────────────────────────────
  private initMap(): void {
    this.map = L.map('map', {
      center: [49.0, 31.5],
      zoom:   6,
      attributionControl: true,
      zoomControl:        false
    });

    L.control.zoom({ position: 'bottomright' }).addTo(this.map);

    // CartoDB Voyager — кольоровий, сучасний, без API-ключа
    L.tileLayer(
        'https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png',
        {
          attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a> &copy; <a href="https://carto.com/">CARTO</a>',
          subdomains:  'abcd',
          maxZoom:     20
        }
    ).addTo(this.map);

    this.clusterGroup = L.markerClusterGroup({
      chunkedLoading:      true,
      maxClusterRadius:    55,
      showCoverageOnHover: false,
      iconCreateFunction:  (cluster: any) => L.divIcon({
        html:      `<div class="sw-cluster"><span>${cluster.getChildCount()}</span></div>`,
        className: '',
        iconSize:  L.point(44, 44)
      })
    });

    this.map.addLayer(this.clusterGroup);
  }

  // ─── Global donate bridge ────────────────────────────────────────────────
  private listenToGlobalDonateEvent(): void {
    const handler = (e: Event) => {
      const helpRequestId = (e as CustomEvent<string>).detail;
      const marker = this.allMarkers.find(m => m.helpRequestId === helpRequestId);
      if (marker) {
        this.selectedMarker.set(marker);
        this.showDonate.set(true);
      }
    };
    window.addEventListener('sw:donate', handler);
    this.destroy$.subscribe(() => window.removeEventListener('sw:donate', handler));
  }

  // ─── Filter → load markers ───────────────────────────────────────────────
  private listenToFilterChanges(): void {
    this.filterChange$.pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
        switchMap(filter => {
          this.isLoading.set(true);
          return this.mapDataService.getMarkers(filter);
        }),
        takeUntil(this.destroy$)
    ).subscribe({
      next: (result: PagedResult<MapMarkerDto>) => {
        this.allMarkers = result.items;
        this.renderMarkers(this.allMarkers);
        this.totalCount.set(result.total);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  // ─── Render pins ─────────────────────────────────────────────────────────
  private renderMarkers(markers: MapMarkerDto[]): void {
    this.clusterGroup.clearLayers();
    this.activeLeafletMarker = null;

    markers.forEach((m: MapMarkerDto) => {
      if (!m.latitude || !m.longitude) return;

      const style   = getTypeStyle(m.supportTypeName);
      const percent = m.targetAmount > 0
          ? Math.min(100, Math.round((m.collectedAmount / m.targetAmount) * 100))
          : 0;

      // Розмір піну залежить від суми збору
      const size   = calcPinSize(m.targetAmount);
      // Висота = size * (50/36) — зберігаємо пропорцію viewBox 36×50
      const height = Math.round(size * (50 / 36));

      const icon = L.divIcon({
        html:        `<div class="sw-pin-wrapper">${buildPinSvg(style.color, percent, size)}</div>`,
        className:   '',
        iconSize:    [size, height],
        iconAnchor:  [size / 2, height],
        popupAnchor: [0, -(height + 4)]
      });

      const leafletMarker = L.marker([m.latitude, m.longitude], { icon });

      leafletMarker.bindTooltip(
          `<div class="sw-tooltip">
           <span class="sw-tooltip-title">${m.title}</span>
           <span class="sw-tooltip-sub">${m.requestItemName} &middot; ${m.supportTypeName}</span>
         </div>`,
          { permanent: false, direction: 'top', offset: [0, -(height + 4)], className: 'sw-tooltip-wrap' }
      );

      leafletMarker.on('click', () => this.onMarkerClick(m, leafletMarker));
      this.clusterGroup.addLayer(leafletMarker);
    });
  }

  // ─── Marker click ────────────────────────────────────────────────────────
  private onMarkerClick(marker: MapMarkerDto, leafletMarker: any): void {
    if (this.activeLeafletMarker) {
      const el = this.activeLeafletMarker.getElement();
      el?.querySelector('.sw-pin-wrapper')?.classList.remove('sw-pin-wrapper--selected');
    }
    this.activeLeafletMarker = leafletMarker;
    leafletMarker.getElement()
        ?.querySelector('.sw-pin-wrapper')
        ?.classList.add('sw-pin-wrapper--selected');

    this.lastMarker = marker;
    this.selectedDetails.set(null);
    this.detailsError.set(null);
    this.isDetailLoading.set(true);
    this.selectedMarker.set(marker);
    this.cdr.markForCheck();
    this.loadDetails(marker.helpRequestId);
  }

  private loadDetails(helpRequestId: string): void {
    this.helpRequestService.getDetails(helpRequestId).subscribe({
      next: (details: HelpRequestDetails) => {
        this.selectedDetails.set(details);
        this.isDetailLoading.set(false);
        this.cdr.markForCheck();
      },
      error: () => {
        this.detailsError.set('Не вдалося завантажити деталі запиту.');
        this.isDetailLoading.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  // ─── Public handlers ─────────────────────────────────────────────────────
  onFiltersChanged(filters: MapFilterParams): void { this.filterChange$.next(filters); }

  retryDetails(): void {
    if (!this.lastMarker) return;
    this.detailsError.set(null);
    this.isDetailLoading.set(true);
    this.loadDetails(this.lastMarker.helpRequestId);
  }

  openDonateFromPanel(): void { if (this.selectedMarker()) this.showDonate.set(true); }
  closeDonate(): void         { this.showDonate.set(false); }

  closePanel(): void {
    if (this.activeLeafletMarker) {
      this.activeLeafletMarker.getElement()
          ?.querySelector('.sw-pin-wrapper')
          ?.classList.remove('sw-pin-wrapper--selected');
      this.activeLeafletMarker = null;
    }
    this.selectedDetails.set(null);
    this.selectedMarker.set(null);
    this.detailsError.set(null);
    this.lastMarker = null;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) this.map.remove();
  }
}
