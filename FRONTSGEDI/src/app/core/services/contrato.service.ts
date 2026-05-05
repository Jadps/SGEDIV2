import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ContratoDto, CreateContratoRequest, ContratoCatalogsDto } from '../models/contrato.dto';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ContratoService {
  private readonly http = inject(HttpClient);

  getContrato(alumnoId: string, materiaId: string, context?: HttpContext): Observable<ContratoDto> {
    return this.http.get<ContratoDto>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.GET}`
        .replace('{alumnoId}', alumnoId)
        .replace('{materiaId}', materiaId),
      { context }
    );
  }

  createContrato(request: CreateContratoRequest): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.BASE}`, request);
  }

  updateContrato(id: string, request: CreateContratoRequest): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.RESPOND.replace('{id}/responder', id)}`, request);
  }

  respondToContrato(contratoId: string, aceptado: boolean, observaciones?: string): Observable<void> {
    return this.http.patch<void>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.RESPOND.replace('{id}', contratoId)}`,
      { aceptado, observaciones }
    );
  }

  getCatalogs(): Observable<ContratoCatalogsDto> {
    return this.http.get<ContratoCatalogsDto>(`${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.CATALOGS}`);
  }
}
