import {
  Component, OnInit, OnDestroy, inject, signal, ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';
import { Subject, switchMap, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { MapDataService } from '../../services/map-data.service';
import { MapFilterComponent } from '../map-filter/map-filter.component';
import { DonateModalComponent } from '../donate-modal/donate-modal.component';
import { MapFilterParams, RequestMapDto, getPrimaryTypeStyle } from '../../models/map.models';

@Component({
  selector: 'app-map',
  standalone: true,
  imports: [CommonModule, MapFilterComponent, DonateModalComponent],
  templateUrl: './map.component.html',
  styleUrls: ['./map.component.scss']
})
export class MapComponent implements OnInit, OnDestroy {
  private readonly mapDataService = inject(MapDataService);
  private readonly cdr            = inject(ChangeDetectorRef);

  private map!: L.Map;
  private clusterGroup: any;
  private destroy$     = new Subject<void>();
  private filterChange$ = new Subject<MapFilterParams>();
  private allRequests: RequestMapDto[] = [];

  readonly isLoading       = signal(false);
  readonly totalCount      = signal(0);
  readonly selectedRequest = signal<RequestMapDto | null>(null);
  readonly showDonate      = signal(false);
  readonly panelRequest    = signal<RequestMapDto | null>(null);

  ngOnInit(): void {
    this.initMap();
    this.listenToGlobalDonateEvent();
    this.listenToFilterChanges();
    this.filterChange$.next({});
  }

  private initMap(): void {
    this.map = L.map('map', { center: [49.0, 31.5], zoom: 6, attributionControl: false });

    L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
      subdomains: 'abcd', maxZoom: 20
    }).addTo(this.map);

    this.clusterGroup = (L as any).markerClusterGroup({
      chunkedLoading: true,
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

  private listenToGlobalDonateEvent(): void {
    const handler = (e: Event) => {
      const id  = (e as CustomEvent<string>).detail;
      const req = this.allRequests.find(r => r.id === id);
      if (req) { this.selectedRequest.set(req); this.showDonate.set(true); }
    };
    window.addEventListener('sw:donate', handler);
    this.destroy$.subscribe(() => window.removeEventListener('sw:donate', handler));
  }

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
      next: result => {
        this.allRequests = result.items as RequestMapDto[];
        this.renderMarkers(this.allRequests);
        this.totalCount.set(result.total);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  private renderMarkers(requests: RequestMapDto[]): void {
    this.clusterGroup.clearLayers();

    requests.forEach(req => {
      if (!req.latitude || !req.longitude) return;
      const style   = getPrimaryTypeStyle(req.supportTypes);
      const percent = req.targetAmount > 0
        ? Math.min(100, Math.round((req.collectedAmount / req.targetAmount) * 100))
        : 0;

      const icon = L.divIcon({
        html: `<div class="sw-pin" style="--c:${style.color}">
                 <span>${style.icon}</span>
                 <div class="sw-pin-bar" style="width:${percent}%"></div>
               </div>`,
        className: '',
        iconSize:   [40, 48],
        iconAnchor: [20, 48],
        popupAnchor:[0, -52]
      });

      const marker = L.marker([req.latitude, req.longitude], { icon });

      // Click: open side panel
      marker.on('click', () => {
        this.panelRequest.set(req);
        this.cdr.markForCheck();
      });

      marker.bindTooltip(req.title, { permanent: false, direction: 'top', offset: [0, -50] });
      this.clusterGroup.addLayer(marker);
    });
  }

  onFiltersChanged(filters: MapFilterParams): void { this.filterChange$.next(filters); }

  openDonateFromPanel(): void {
    if (this.panelRequest()) {
      this.selectedRequest.set(this.panelRequest());
      this.showDonate.set(true);
    }
  }

  closeDonate(): void { this.showDonate.set(false); this.selectedRequest.set(null); }
  closePanel(): void  { this.panelRequest.set(null); }

  get panelPercent(): number {
    const r = this.panelRequest();
    if (!r || !r.targetAmount) return 0;
    return Math.min(100, Math.round((r.collectedAmount / r.targetAmount) * 100));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.map?.remove();
  }
}
