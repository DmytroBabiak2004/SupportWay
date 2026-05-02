import {
  Component, EventEmitter, Output, inject,
  OnInit, OnDestroy, AfterViewInit, ChangeDetectorRef, NgZone
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, of, takeUntil } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { HelpRequestService, LocationPayload } from '../../services/help-request.service';
import { LocationService, NominatimResult } from '../../services/location.service';
import { environment } from '../../../environments/environment.development';

declare const L: any;

export type LocationMode = 'none' | 'search' | 'map';

export interface SupportType {
  id: string;
  nameOfType: string;
}

export interface LocalRequestItem {
  tempId: string;
  supportTypeId: string;
  supportTypeName: string;
  name: string;
  quantity: number;
  unitPrice: number;
}

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
  private http = inject(HttpClient);
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

  requestItems: LocalRequestItem[] = [];
  supportTypes: SupportType[] = [];
  showItemModal = false;
  isLoadingSupportTypes = false;

  itemSupportTypeId = '';
  itemName = '';
  itemQuantity: number = 1;
  itemUnitPrice: number = 0;
  itemFormError = '';

  preferredDonationMethod: 'bank_card' | 'iban' | 'payment_link' = 'bank_card';
  donationRecipientName = '';
  donationRecipientCardNumber = '';
  donationRecipientIban = '';
  donationPaymentLink = '';
  donationNotes = '';

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

    this.loadSupportTypes();
  }

  ngAfterViewInit(): void {}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) { this.map.remove(); this.map = null; }
  }

  loadSupportTypes(): void {
    this.isLoadingSupportTypes = true;
    this.http.get<SupportType[]>(`${environment.apiUrl}/SupportTypes`)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: types => {
            this.supportTypes = types;
            this.isLoadingSupportTypes = false;
            if (types.length > 0) this.itemSupportTypeId = types[0].id;
            this.cdr.detectChanges();
          },
          error: () => { this.isLoadingSupportTypes = false; }
        });
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

  onSearchInput(): void { this.searchSubject.next(this.searchQuery); }

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
      link.id = 'leaflet-css'; link.rel = 'stylesheet';
      link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
      document.head.appendChild(link);
      const script = document.createElement('script');
      script.src = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.js';
      script.onload = () => resolve();
      document.head.appendChild(script);
    });
  }

  private fixLeafletIcons(): void {
    // Виправляє помилку 404 для іконок при завантаженні через CDN
    const iconDefault = L.icon({
      iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
      iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
      shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      tooltipAnchor: [16, -28],
      shadowSize: [41, 41]
    });
    L.Marker.prototype.options.icon = iconDefault;
  }

  private buildMap(): void {
    this.zone.runOutsideAngular(() => {
      const el = document.getElementById('location-map');
      if (!el) return;

      this.fixLeafletIcons();

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
            latitude: Number(lat), // Гарантуємо число
            longitude: Number(lng), // Гарантуємо число
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
            latitude: Number(lat),
            longitude: Number(lng),
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

  openItemModal(): void {
    this.itemSupportTypeId = this.supportTypes[0]?.id ?? '';
    this.itemName = '';
    this.itemQuantity = 1;
    this.itemUnitPrice = 0;
    this.itemFormError = '';
    this.showItemModal = true;
  }

  closeItemModal(): void { this.showItemModal = false; }

  getSupportTypeName(id: string): string {
    return this.supportTypes.find(t => t.id === id)?.nameOfType ?? '';
  }

  confirmAddItem(): void {
    if (!this.itemSupportTypeId) { this.itemFormError = 'Оберіть тип допомоги'; return; }
    if (!this.itemName.trim()) { this.itemFormError = "Введіть назву пункту"; return; }
    if (this.itemQuantity < 1) { this.itemFormError = 'Кількість має бути > 0'; return; }

    this.requestItems.push({
      tempId: Math.random().toString(36).slice(2),
      supportTypeId: this.itemSupportTypeId,
      supportTypeName: this.getSupportTypeName(this.itemSupportTypeId),
      name: this.itemName.trim(),
      quantity: this.itemQuantity,
      unitPrice: this.itemUnitPrice ?? 0
    });
    this.showItemModal = false;
  }

  removeItem(tempId: string): void {
    this.requestItems = this.requestItems.filter(i => i.tempId !== tempId);
  }

  submit(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const { title, content } = this.createForm.getRawValue();

    // ВАЖЛИВО: Якщо ви використовуєте FormData для файлів,
    // сервіс повинен правильно обробляти типи даних.
    const payload = {
      title: title!,
      content: content!,
      latitude: this.selectedLocation ? Number(this.selectedLocation.latitude) : undefined,
      longitude: this.selectedLocation ? Number(this.selectedLocation.longitude) : undefined,
      address: this.selectedLocation?.address,
      districtName: this.selectedLocation?.districtName,
      file: this.selectedFile ?? undefined,
      preferredDonationMethod: this.preferredDonationMethod,
      donationRecipientName: this.donationRecipientName.trim() || undefined,
      donationRecipientCardNumber: this.donationRecipientCardNumber.trim() || undefined,
      donationRecipientIban: this.donationRecipientIban.trim() || undefined,
      donationPaymentLink: this.donationPaymentLink.trim() || undefined,
      donationNotes: this.donationNotes.trim() || undefined
    };

    this.helpService.createHelpRequestDirect(payload).subscribe({
      next: (response: any) => {
        const helpRequestId = response?.id ?? null;

        if (this.requestItems.length > 0 && helpRequestId) {
          const posts = this.requestItems.map(item =>
              this.http.post(`${environment.apiUrl}/RequestItems`, {
                helpRequestId,
                supportTypeId: item.supportTypeId,
                name: item.name,
                quantity: Number(item.quantity),
                unitPrice: Number(item.unitPrice)
              }).toPromise()
          );

          Promise.all(posts).finally(() => {
            this.isSubmitting = false;
            this.created.emit();
            this.close.emit();
          });
        } else {
          this.isSubmitting = false;
          this.created.emit();
          this.close.emit();
        }
      },
      error: err => {
        console.error('Помилка сервера:', err.error);
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void { this.close.emit(); }
}
