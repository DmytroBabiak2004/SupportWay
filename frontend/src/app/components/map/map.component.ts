import {
  Component, OnInit, OnDestroy, inject, signal, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, switchMap, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { MapDataService }        from '../../services/map-data.service';
import { HelpRequestService }    from '../../services/help-request.service';
import { MapFilterComponent }    from './map-filter/map-filter.component';
import { DonateModalComponent }  from '../../dialog/donate-modal/donate-modal.component';
import { MapMetaPillComponent }      from './map-meta-pill/map-meta-pill.component';
import { MapSidePanelComponent }     from './map-side-panel/map-side-panel.component';
import { MapPanelHeaderComponent }   from './map-panel-header/map-panel-header.component';
import { MapPanelProgressComponent } from './map-panel-progress/map-panel-progress.component';
import { MapPanelItemsComponent }    from './map-panel-items/map-panel-items.component';
import { MapPanelActionsComponent }  from './map-panel-actions/map-panel-actions.component';

import { MapFilterParams, MapMarkerDto, PagedResult, getTypeStyle } from '../../models/map.models';
import { HelpRequestDetails } from '../../models/help-request.model';

declare const L: any;

// ─── Build a clean SVG map pin (no emoji) ────────────────────────
function buildPinSvg(color: string, progress: number): string {
  const pct   = Math.max(0, Math.min(100, progress));
  const r     = 9;
  const circ  = 2 * Math.PI * r;
  const dash  = (pct / 100) * circ;

  return `
    <svg class="sw-pin-svg" viewBox="0 0 32 44" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <filter id="ps" x="-40%" y="-20%" width="180%" height="160%">
          <feDropShadow dx="0" dy="2" stdDeviation="2.5" flood-color="rgba(0,0,0,0.25)"/>
        </filter>
      </defs>
      <path d="M16 2 C8.268 2 2 8.268 2 16 C2 24.5 16 42 16 42 C16 42 30 24.5 30 16 C30 8.268 23.732 2 16 2 Z"
            fill="${color}" filter="url(#ps)"/>
      <circle cx="16" cy="16" r="9" fill="white" opacity="0.95"/>
      <circle cx="16" cy="16" r="${r}"
              fill="none"
              stroke="${color}"
              stroke-width="2"
              stroke-dasharray="${dash} ${circ}"
              stroke-dashoffset="${circ * 0.25}"
              stroke-linecap="round"
              opacity="0.7"/>
      <circle cx="16" cy="16" r="3.5" fill="${color}"/>
    </svg>
  `.trim();
}

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [
    CommonModule,
    MapFilterComponent,
    DonateModalComponent,
    MapMetaPillComponent,
    MapSidePanelComponent,
    MapPanelHeaderComponent,
    MapPanelProgressComponent,
    MapPanelItemsComponent,
    MapPanelActionsComponent,
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

  // ─────────────────────────────────────────
  ngOnInit(): void {
    this.initMap();
    this.listenToGlobalDonateEvent();
    this.listenToFilterChanges();
    this.filterChange$.next({});
  }

  // ─── Map init ────────────────────────────
  private initMap(): void {
    this.map = L.map('map', {
      center: [49.0, 31.5],
      zoom: 6,
      attributionControl: true,
      zoomControl: false
    });

    L.control.zoom({ position: 'bottomright' }).addTo(this.map);

    L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
      subdomains: 'abcd',
      maxZoom: 20
    }).addTo(this.map);

    this.clusterGroup = L.markerClusterGroup({
      chunkedLoading:      true,
      maxClusterRadius:    60,
      showCoverageOnHover: false,
      iconCreateFunction:  (cluster: any) => L.divIcon({
        html:      `<div class="sw-cluster">${cluster.getChildCount()}</div>`,
        className: '',
        iconSize:  L.point(40, 40)
      })
    });

    this.map.addLayer(this.clusterGroup);
  }

  // ─── Global donate event ─────────────────
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

  // ─── Filter → load markers ───────────────
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

  // ─── Render SVG pins ──────────────────────
  private renderMarkers(markers: MapMarkerDto[]): void {
    this.clusterGroup.clearLayers();

    markers.forEach((m: MapMarkerDto) => {
      if (!m.latitude || !m.longitude) return;

      const style   = getTypeStyle(m.supportTypeName);
      const percent = m.targetAmount > 0
        ? Math.min(100, Math.round((m.collectedAmount / m.targetAmount) * 100))
        : 0;

      const icon = L.divIcon({
        html:        `<div class="sw-pin">${buildPinSvg(style.color, percent)}</div>`,
        className:   '',
        iconSize:    [32, 44],
        iconAnchor:  [16, 44],
        popupAnchor: [0, -48]
      });

      const leafletMarker = L.marker([m.latitude, m.longitude], { icon });

      leafletMarker.bindTooltip(
        `<b>${m.title}</b><br>${m.requestItemName} · ${m.supportTypeName}`,
        { permanent: false, direction: 'top', offset: [0, -48] }
      );

      leafletMarker.on('click', () => this.onMarkerClick(m));
      this.clusterGroup.addLayer(leafletMarker);
    });
  }

  // ─── Marker click ────────────────────────
  private onMarkerClick(marker: MapMarkerDto): void {
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

  // ─── Public handlers ─────────────────────
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
    this.selectedDetails.set(null);
    this.selectedMarker.set(null);
    this.detailsError.set(null);
    this.lastMarker = null;
  }

  getImageSrc(base64?: string | null): string | null {
    return this.helpRequestService.getImageSrc(base64);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) this.map.remove();
  }
}
