import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';

@Injectable({ providedIn: 'root' })
export class ExpedienteService {
  private readonly http = inject(HttpClient);

  getExpediente(alumnoId: string, semestre?: string): Observable<any[]> {
    let params = new HttpParams();
    if (semestre) params = params.set('semestre', semestre);
    return this.http.get<any[]>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.EXPEDIENTE.replace('{id}', alumnoId)}`,
      { params }
    );
  }

  getSemestres(alumnoId: string): Observable<string[]> {
    return this.http.get<string[]>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.SEMESTRES.replace('{id}', alumnoId)}`
    );
  }

  getMyDeadlines(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}${API_ENDPOINTS.STUDENTS.MY_DEADLINES}`);
  }

  extendDeadline(payload: {
    acuerdoId?: string;
    alumnoId?: string;
    tipoAcuerdo?: number;
    semestre?: string;
    nuevaFechaLimite: Date;
  }): Observable<void> {
    return this.http.patch<void>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.PRORROGA}`,
      { ...payload, nuevaFechaLimite: payload.nuevaFechaLimite.toISOString() }
    );
  }

  reviewDocument(alumnoId: string, docId: string, aprobado: boolean, motivoRechazo?: string): Observable<void> {
    return this.http.patch<void>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.DOCUMENTS.replace('{id}', alumnoId)}/${docId}/revisar`,
      { aprobado, motivoRechazo }
    );
  }
}
