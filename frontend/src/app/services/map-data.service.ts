import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { MapFilterParams, PagedResult, RequestMapDto } from '../models/map.models';

@Injectable({ providedIn: 'root' })
export class MapDataService {
  private readonly http    = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/map`;

  getMarkers(filter: MapFilterParams): Observable<PagedResult<RequestMapDto>> {
    let params = new HttpParams();

    if (filter.supportTypeId)
      params = params.set('supportTypeId', filter.supportTypeId);
    if (filter.isActive !== undefined && filter.isActive !== null)
      params = params.set('isActive', filter.isActive);
    if (filter.region)
      params = params.set('region', filter.region);
    if (filter.maxTarget !== undefined)
      params = params.set('maxTarget', filter.maxTarget);
    if (filter.minCollected !== undefined)
      params = params.set('minCollected', filter.minCollected);

    params = params
      .set('page', filter.page ?? 1)
      .set('size', filter.size ?? 200);

    return this.http.get<PagedResult<RequestMapDto>>(this.baseUrl, { params });
  }
}
