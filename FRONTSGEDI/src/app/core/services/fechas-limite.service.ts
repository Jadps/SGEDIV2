import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface FechaLimiteDto {
  tipoAcuerdo: number;
  fechaLimite: string;
  isDefault: boolean;
}

export interface UpdateFechasLimiteRequest {
  carreraId: number;
  semestre?: string;
  fechas: { tipoAcuerdo: number; fechaLimite: string }[];
}

@Injectable({
  providedIn: 'root'
})
export class FechasLimiteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/fechas-limite`;

  getFechasLimite(carreraId: number, semestre?: string): Observable<any> {
    let params = new HttpParams().set('carreraId', carreraId.toString());
    if (semestre) {
      params = params.set('semestre', semestre);
    }
    return this.http.get<any>(this.baseUrl, { params });
  }

  updateFechasLimite(request: UpdateFechasLimiteRequest): Observable<void> {
    return this.http.put<void>(this.baseUrl, request);
  }
}
