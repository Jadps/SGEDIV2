import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ModuleDto } from '../models/module.dto';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly http = inject(HttpClient);

  getMenuModules(): Observable<ModuleDto[]> {
    return this.http.get<ModuleDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MODULES}`);
  }
}
