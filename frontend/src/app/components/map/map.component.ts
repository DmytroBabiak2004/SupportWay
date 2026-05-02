import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, switchMap, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import { MapDataService } from '../../services/map-data.service';
import { HelpRequestService } from '../../services/help-request.service';
import { MapFilterComponent } from './map-filter/map-filter.component';
import { DonateModalComponent } from '../../dialog/donate-modal/donate-modal.component';
import { MapDetailsPanelComponent } from './map-details-panel/map-details-panel.component';

import {
  MapFilterParams,
  MapMarkerDto,
  PagedResult,
  getTypeStyle
} from '../../models/map.models';
import { HelpRequestDetails } from '../../models/help-request.model';

declare const L: any;

const PIN_MIN_SIZE = 28;
const PIN_MAX_SIZE = 52;
const AMOUNT_MIN = 1_000;
const AMOUNT_MAX = 200_000;

function calcPinSize(targetAmount: number): number {
  if (!targetAmount || targetAmount <= 0) return PIN_MIN_SIZE;
  if (targetAmount <= AMOUNT_MIN) return PIN_MIN_SIZE;
  if (targetAmount >= AMOUNT_MAX) return PIN_MAX_SIZE;

  const logMin = Math.log(AMOUNT_MIN);
  const logMax = Math.log(AMOUNT_MAX);
  const logVal = Math.log(targetAmount);
  const t = (logVal - logMin) / (logMax - logMin);

  return Math.round(PIN_MIN_SIZE + t * (PIN_MAX_SIZE - PIN_MIN_SIZE));
}

function buildPinSvg(color: string, progress: number, size: number): string {
  const pct = Math.max(0, Math.min(100, progress));
  const scale = size / 36;
  const r = Math.round(8 * scale);
  const circ = 2 * Math.PI * r;
  const dash = (pct / 100) * circ;
  const offset = (circ * 0.25).toFixed(2);
  const cx = 18;
  const cy = 16;
  const innerR = Math.round(9.5 * scale);
  const dotR = Math.round(3.5 * scale);

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

function toRadians(value: number): number {
  return (value * Math.PI) / 180;
}

function calculateDistanceKm(
  lat1: number,
  lon1: number,
  lat2: number,
  lon2: number
): number {
  const earthRadiusKm = 6371;
  const dLat = toRadians(lat2 - lat1);
  const dLon = toRadians(lon2 - lon1);

  const a =
    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos(toRadians(lat1)) *
      Math.cos(toRadians(lat2)) *
      Math.sin(dLon / 2) *
      Math.sin(dLon / 2);

  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  return Number((earthRadiusKm * c).toFixed(1));
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
  styleUrls: ['./map.component.scss']
})
export class MapComponent implements OnInit, OnDestroy {
  private readonly mapDataService = inject(MapDataService);
  private readonly helpRequestService = inject(HelpRequestService);
  private readonly cdr = inject(ChangeDetectorRef);

  private map!: any;
  private clusterGroup: any;
  private activeLeafletMarker: any = null;
  private myLocationMarker: any = null;
  private destroy$ = new Subject<void>();
  private filterChange$ = new Subject<MapFilterParams>();
  private lastMarker: MapMarkerDto | null = null;
  private allMarkers: MapMarkerDto[] = [];

  readonly isLoading = signal(false);
  readonly isDetailLoading = signal(false);
  readonly totalCount = signal(0);
  readonly selectedMarker = signal<MapMarkerDto | null>(null);
  readonly selectedDetails = signal<HelpRequestDetails | null>(null);
  readonly detailsError = signal<string | null>(null);
  readonly showDonate = signal(false);
  readonly myLocation = signal<{ latitude: number; longitude: number } | null>(null);
  readonly locationError = signal<string | null>(null);

  ngOnInit(): void {
    this.initMap();
    this.requestMyLocation();
    this.listenToGlobalDonateEvent();
    this.listenToFilterChanges();
    this.filterChange$.next({});
  }

  private initMap(): void {
    this.map = L.map('map', {
      center: [49.0, 31.5],
      zoom: 6,
      attributionControl: true,
      zoomControl: false
    });

    L.control.zoom({ position: 'bottomright' }).addTo(this.map);

    L.tileLayer(
      'https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png',
      {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a> &copy; <a href="https://carto.com/">CARTO</a>',
        subdomains: 'abcd',
        maxZoom: 20
      }
    ).addTo(this.map);

    this.clusterGroup = L.markerClusterGroup({
      chunkedLoading: true,
      maxClusterRadius: 55,
      showCoverageOnHover: false,
      iconCreateFunction: (cluster: any) => L.divIcon({
        html: `<div class="sw-cluster"><span>${cluster.getChildCount()}</span></div>`,
        className: '',
        iconSize: L.point(44, 44)
      })
    });

    this.map.addLayer(this.clusterGroup);
  }

  private requestMyLocation(): void {
    if (!navigator.geolocation) {
      this.locationError.set('Ваш браузер не підтримує геолокацію.');
      this.cdr.markForCheck();
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        const latitude = position.coords.latitude;
        const longitude = position.coords.longitude;

        this.myLocation.set({ latitude, longitude });
        this.locationError.set(null);
        this.renderMyLocationMarker(latitude, longitude);
        this.refreshMarkersWithDistances();
        this.cdr.markForCheck();
      },
      (error) => {
        const message = error.code === error.PERMISSION_DENIED
          ? 'Геолокацію вимкнено. Увімкніть доступ до місцезнаходження, щоб бачити свою позицію і відстань до запитів.'
          : 'Не вдалося отримати вашу геопозицію.';

        this.locationError.set(message);
        this.clearMyLocationMarker();
        this.refreshMarkersWithDistances();
        this.cdr.markForCheck();
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 0
      }
    );
  }

  private renderMyLocationMarker(latitude: number, longitude: number): void {
    this.clearMyLocationMarker();

    const myIcon = L.divIcon({
      html: `
        <div style="
          width: 18px;
          height: 18px;
          border-radius: 999px;
          background: #2563eb;
          border: 3px solid white;
          box-shadow: 0 0 0 8px rgba(37,99,235,0.18);
        "></div>
      `,
      className: '',
      iconSize: [18, 18],
      iconAnchor: [9, 9]
    });

    this.myLocationMarker = L.marker([latitude, longitude], { icon: myIcon })
      .bindTooltip('Ваша позиція', {
        permanent: false,
        direction: 'top',
        offset: [0, -10]
      })
      .addTo(this.map);
  }

  private clearMyLocationMarker(): void {
    if (this.myLocationMarker) {
      this.map.removeLayer(this.myLocationMarker);
      this.myLocationMarker = null;
    }
  }

  private listenToGlobalDonateEvent(): void {
    const handler = (e: Event) => {
      const helpRequestId = (e as CustomEvent<string>).detail;
      const marker = this.allMarkers.find((item) => item.helpRequestId === helpRequestId) ?? null;

      if (!marker) return;

      this.selectedMarker.set(marker);
      this.lastMarker = marker;
      this.showDonate.set(true);
      this.cdr.markForCheck();
    };

    window.addEventListener('sw:donate', handler);
    this.destroy$.subscribe(() => window.removeEventListener('sw:donate', handler));
  }

  private listenToFilterChanges(): void {
    this.filterChange$.pipe(
      debounceTime(300),
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
      switchMap((filter) => {
        this.isLoading.set(true);
        return this.mapDataService.getMarkers(filter);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (result: PagedResult<MapMarkerDto>) => {
        this.totalCount.set(result.total);
        this.isLoading.set(false);
        this.allMarkers = this.enrichMarkersWithDistance(result.items);
        this.renderMarkers(this.allMarkers);
        this.syncSelectedMarkerWithCurrentCollection();
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading.set(false);
        this.cdr.markForCheck();
      }
    });
  }

  private enrichMarkersWithDistance(markers: MapMarkerDto[]): MapMarkerDto[] {
    const location = this.myLocation();

    return markers.map((marker) => {
      const nextMarker: MapMarkerDto = { ...marker };

      if (
        location &&
        nextMarker.latitude != null &&
        nextMarker.longitude != null
      ) {
        nextMarker.distanceKm = calculateDistanceKm(
          location.latitude,
          location.longitude,
          nextMarker.latitude,
          nextMarker.longitude
        );
      } else {
        nextMarker.distanceKm = undefined;
      }

      return nextMarker;
    });
  }

  private refreshMarkersWithDistances(): void {
    this.allMarkers = this.enrichMarkersWithDistance(this.allMarkers);
    this.renderMarkers(this.allMarkers);
    this.syncSelectedMarkerWithCurrentCollection();
  }

  private syncSelectedMarkerWithCurrentCollection(): void {
    const currentSelected = this.selectedMarker();
    if (!currentSelected) return;

    const nextSelected = this.allMarkers.find(
      (marker) =>
        marker.helpRequestId === currentSelected.helpRequestId &&
        marker.requestItemId === currentSelected.requestItemId
    ) ?? null;

    this.selectedMarker.set(nextSelected);
    this.lastMarker = nextSelected;
  }

  private renderMarkers(markers: MapMarkerDto[]): void {
    this.clusterGroup.clearLayers();
    this.activeLeafletMarker = null;

    markers.forEach((marker) => {
      if (marker.latitude == null || marker.longitude == null) return;

      const style = getTypeStyle(marker.supportTypeName);
      const percent = marker.targetAmount > 0
        ? Math.min(100, Math.round((marker.collectedAmount / marker.targetAmount) * 100))
        : 0;

      const size = calcPinSize(marker.targetAmount);
      const height = Math.round(size * (50 / 36));

      const icon = L.divIcon({
        html: `<div class="sw-pin-wrapper">${buildPinSvg(style.color, percent, size)}</div>`,
        className: '',
        iconSize: [size, height],
        iconAnchor: [size / 2, height],
        popupAnchor: [0, -(height + 4)]
      });

      const tooltipDistance = marker.distanceKm != null
        ? `<span class="sw-tooltip-sub">Від вас: ${this.formatDistance(marker.distanceKm)}</span>`
        : '';

      const leafletMarker = L.marker([marker.latitude, marker.longitude], { icon });
      leafletMarker.bindTooltip(
        `<div class="sw-tooltip">
           <span class="sw-tooltip-title">${marker.title}</span>
           <span class="sw-tooltip-sub">${marker.requestItemName} &middot; ${marker.supportTypeName}</span>
           ${tooltipDistance}
         </div>`,
        {
          permanent: false,
          direction: 'top',
          offset: [0, -(height + 4)],
          className: 'sw-tooltip-wrap'
        }
      );

      leafletMarker.on('click', () => this.onMarkerClick(marker, leafletMarker));
      this.clusterGroup.addLayer(leafletMarker);

      const isSelected = this.selectedMarker()?.helpRequestId === marker.helpRequestId
        && this.selectedMarker()?.requestItemId === marker.requestItemId;

      if (isSelected) {
        this.activeLeafletMarker = leafletMarker;
        leafletMarker.getElement()?.querySelector('.sw-pin-wrapper')?.classList.add('sw-pin-wrapper--selected');
      }
    });
  }

  private onMarkerClick(marker: MapMarkerDto, leafletMarker: any): void {
    if (this.activeLeafletMarker) {
      this.activeLeafletMarker.getElement()
        ?.querySelector('.sw-pin-wrapper')
        ?.classList.remove('sw-pin-wrapper--selected');
    }

    this.activeLeafletMarker = leafletMarker;
    leafletMarker.getElement()
      ?.querySelector('.sw-pin-wrapper')
      ?.classList.add('sw-pin-wrapper--selected');

    this.lastMarker = marker;
    this.selectedMarker.set(marker);
    this.selectedDetails.set(null);
    this.detailsError.set(null);
    this.isDetailLoading.set(true);
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

  onFiltersChanged(filters: MapFilterParams): void {
    this.filterChange$.next(filters);
  }

  retryDetails(): void {
    if (!this.lastMarker) return;

    this.detailsError.set(null);
    this.isDetailLoading.set(true);
    this.loadDetails(this.lastMarker.helpRequestId);
  }

  detectMyLocation(): void {
    this.requestMyLocation();
  }

  openDonateFromPanel(): void {
    if (this.selectedMarker()) {
      this.showDonate.set(true);
    }
  }

  closeDonate(): void {
    this.showDonate.set(false);
  }

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
    this.cdr.markForCheck();
  }

  formatDistance(distanceKm: number): string {
    if (distanceKm < 1) {
      return `${Math.round(distanceKm * 1000)} м`;
    }

    return `${distanceKm.toFixed(1)} км`;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    if (this.map) {
      this.map.remove();
    }
  }
}
