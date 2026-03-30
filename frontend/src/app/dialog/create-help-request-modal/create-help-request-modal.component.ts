import {
  Component, EventEmitter, Output, inject,
  OnInit, OnDestroy, AfterViewInit, ChangeDetectorRef, NgZone
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, takeUntil } from 'rxjs';
import { HelpRequestService, LocationPayload } from '../../services/help-request.service';
import { LocationService, NominatimResult } from '../../services/location.service';

declare const L: any;

export type LocationMode = 'none' | 'search' | 'map';

@Component({
  selector: 'app-create-help-request-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  templateUrl: './create-help-request-modal.component.html',
  styleUrls: ['./create-help-request-modal.component.scss']
})
export class CreateHelpRequestModalComponent implements OnInit, OnDestroy, AfterViewInit {
  private fb = inject(FormBuilder);
  private helpService = inject(HelpRequestService);
  private locationService = inject(LocationService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);

  @Output() close = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();

  createForm = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    content: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  locationMode: LocationMode = 'none';
  searchQuery = '';
  searchResults: NominatimResult[] = [];
  isSearching = false;
  selectedLocation: LocationPayload | null = null;
  selectedLocationLabel = '';

  private map: any = null;
  private marker: any = null;
  mapReady = false;

  isSubmitting = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap(q => {
        if (q.trim().length < 2) { this.searchResults = []; return of([]); }
        this.isSearching = true;
        return this.locationService.searchUkrainianCities(q);
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: results => {
        this.isSearching = false;
        this.searchResults = results;
        this.cdr.detectChanges();
      },
      error: () => { this.isSearching = false; }
    });
  }

  ngAfterViewInit(): void {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) { this.map.remove(); this.map = null; }
  }

  setLocationMode(mode: LocationMode): void {
    if (this.locationMode === mode) { this.locationMode = 'none'; return; }
    this.locationMode = mode;
    this.selectedLocation = null;
    this.selectedLocationLabel = '';
    this.searchResults = [];
    this.searchQuery = '';

    if (mode === 'map') {
      this.mapReady = false;
      setTimeout(() => this.initMap(), 150);
    } else {
      if (this.map) { this.map.remove(); this.map = null; }
    }
  }

  onSearchInput(): void {
    this.searchSubject.next(this.searchQuery);
  }

  selectSearchResult(result: NominatimResult): void {
    const addr = result.address;
    const city = addr.city || addr.town || addr.village || addr.county || '';
    const label = city
      ? `${city}${addr.state ? ', ' + addr.state : ''}`
      : result.display_name;

    this.selectedLocation = {
      latitude: parseFloat(result.lat),
      longitude: parseFloat(result.lon),
      address: result.display_name,
      districtName: label
    };
    this.selectedLocationLabel = label;
    this.searchResults = [];
    this.searchQuery = '';
  }

  clearLocation(): void {
    this.selectedLocation = null;
    this.selectedLocationLabel = '';
    this.searchResults = [];
    this.searchQuery = '';
    if (this.marker) { this.marker.remove(); this.marker = null; }
  }

  private initMap(): void {
    if (typeof L === 'undefined') {
      this.loadLeaflet().then(() => this.buildMap());
    } else {
      this.buildMap();
    }
  }

  private loadLeaflet(): Promise<void> {
    return new Promise((resolve) => {
      if (document.getElementById('leaflet-css')) { resolve(); return; }
      const link = document.createElement('link');
      link.id = 'leaflet-css';
      link.rel = 'stylesheet';
      link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
      document.head.appendChild(link);
      const script = document.createElement('script');
      script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
      script.onload = () => resolve();
      document.head.appendChild(script);
    });
  }

  private buildMap(): void {
    this.zone.runOutsideAngular(() => {
      const el = document.getElementById('location-map');
      if (!el) return;

      this.map = L.map(el, { zoomControl: true }).setView([49.0, 31.5], 6);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
      }).addTo(this.map);

      this.map.on('click', (e: any) => {
        const { lat, lng } = e.latlng;
        if (this.marker) {
          this.marker.setLatLng([lat, lng]);
        } else {
          this.marker = L.marker([lat, lng], { draggable: true }).addTo(this.map);
          this.marker.on('dragend', (ev: any) => {
            const pos = ev.target.getLatLng();
            this.onMapLocationSelected(pos.lat, pos.lng);
          });
        }
        this.onMapLocationSelected(lat, lng);
      });

      this.zone.run(() => { this.mapReady = true; this.cdr.detectChanges(); });
    });
  }

  private onMapLocationSelected(lat: number, lng: number): void {
    this.locationService.reverseGeocode(lat, lng).subscribe({
      next: result => {
        this.zone.run(() => {
          const addr = (result as any).address || {};
          const city = addr.city || addr.town || addr.village || '';
          const label = city
            ? `${city}${addr.state ? ', ' + addr.state : ''}`
            : `${lat.toFixed(4)}, ${lng.toFixed(4)}`;
          this.selectedLocation = {
            latitude: lat, longitude: lng,
            address: (result as any).display_name || `${lat}, ${lng}`,
            districtName: label
          };
          this.selectedLocationLabel = label;
          this.cdr.detectChanges();
        });
      },
      error: () => {
        this.zone.run(() => {
          this.selectedLocation = {
            latitude: lat, longitude: lng,
            districtName: `${lat.toFixed(4)}, ${lng.toFixed(4)}`
          };
          this.selectedLocationLabel = this.selectedLocation.districtName!;
          this.cdr.detectChanges();
        });
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.[0]) {
      this.selectedFile = input.files[0];
      const reader = new FileReader();
      reader.onload = () => { this.imagePreview = reader.result as string; };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  removeImage(): void { this.selectedFile = null; this.imagePreview = null; }

  submit(): void {
    if (this.createForm.invalid) { this.createForm.markAllAsTouched(); return; }
    this.isSubmitting = true;
    const { title, content } = this.createForm.getRawValue();

    this.helpService.createHelpRequest(
      title!, content!,
      this.selectedLocation ?? undefined,
      this.selectedFile ?? undefined
    ).subscribe({
      next: () => { this.isSubmitting = false; this.created.emit(); this.close.emit(); },
      error: err => { console.error('Помилка:', err); this.isSubmitting = false; }
    });
  }

  onCancel(): void { this.close.emit(); }
}
