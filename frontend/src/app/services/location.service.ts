import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface LocationDto {
  locationId: string;
  districtName: string;
  address?: string;
  latitude?: number;
  longitude?: number;
}

export interface NominatimResult {
  place_id: number;
  display_name: string;
  lat: string;
  lon: string;
  address: {
    city?: string;
    town?: string;
    village?: string;
    county?: string;
    state?: string;
    country?: string;
  };
}

@Injectable({ providedIn: 'root' })
export class LocationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Locations`;

  getAll(): Observable<LocationDto[]> {
    return this.http.get<LocationDto[]>(this.apiUrl);
  }

  /** Search Ukrainian cities/settlements via OpenStreetMap Nominatim (free, no key needed) */
  searchUkrainianCities(query: string): Observable<NominatimResult[]> {
    const params = new URLSearchParams({
      q: query,
      countrycodes: 'ua',
      format: 'json',
      addressdetails: '1',
      limit: '7',
      'accept-language': 'uk'
    });
    return this.http.get<NominatimResult[]>(
      `https://nominatim.openstreetmap.org/search?${params.toString()}`,
      { headers: { 'Accept-Language': 'uk' } }
    );
  }

  /** Reverse geocode: coords → address */
  reverseGeocode(lat: number, lon: number): Observable<NominatimResult> {
    const params = new URLSearchParams({
      lat: lat.toString(),
      lon: lon.toString(),
      format: 'json',
      addressdetails: '1',
      'accept-language': 'uk'
    });
    return this.http.get<NominatimResult>(
      `https://nominatim.openstreetmap.org/reverse?${params.toString()}`
    );
  }
}
