import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ModuleDto } from '../models/module.dto';
import { CarreraDto } from '../models/carrera.dto';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly http = inject(HttpClient);

  private readonly carreras$ = this.http.get<CarreraDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.CARRERAS}`).pipe(
    shareReplay(1)
  );

  getMenuModules(): Observable<ModuleDto[]> {
    return this.http.get<ModuleDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MODULES}`);
  }

  getCarreras(): Observable<CarreraDto[]> {
    return this.carreras$;
  }
}
