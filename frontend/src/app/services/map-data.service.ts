import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { MapFilterParams, MapMarkerDto, PagedResult } from '../models/map.models';

@Injectable({ providedIn: 'root' })
export class MapDataService {
  private readonly http    = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/map`;

  getMarkers(filter: MapFilterParams): Observable<PagedResult<MapMarkerDto>> {
    let params = new HttpParams();

    if (filter.supportTypeId)
      params = params.set('supportTypeId', filter.supportTypeId);
    if (filter.isActive !== undefined && filter.isActive !== null)
      params = params.set('isActive', String(filter.isActive));
    if (filter.region)
      params = params.set('region', filter.region);
    if (filter.minCollectedAmount !== undefined)
      params = params.set('minCollectedAmount', filter.minCollectedAmount);
    if (filter.maxTargetAmount !== undefined)
      params = params.set('maxTargetAmount', filter.maxTargetAmount);
    if (filter.search)
      params = params.set('search', filter.search);

    params = params
      .set('page', filter.page ?? 1)
      .set('size', filter.size ?? 200);

    return this.http.get<PagedResult<MapMarkerDto>>(`${this.baseUrl}/markers`, { params });
  }
}
