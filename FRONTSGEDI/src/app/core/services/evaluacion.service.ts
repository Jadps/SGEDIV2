import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';

export interface EvaluacionDto {
  id: string;
  evaluadorNombre: string;
  evaluadorRol: string;
  calificacion: number;
  observaciones: string;
  semestre: string;
  fechaEvaluacion: string;
}

export interface SubmitEvaluacionRequest {
  calificacion: number;
  observaciones: string;
}

@Injectable({ providedIn: 'root' })
export class EvaluacionService {
  private readonly http = inject(HttpClient);

  getEvaluaciones(alumnoId: string): Observable<EvaluacionDto[]> {
    return this.http.get<EvaluacionDto[]>(
      `${environment.apiUrl}${API_ENDPOINTS.EVALUACIONES.BASE.replace('{id}', alumnoId)}`
    );
  }

  submitEvaluacion(alumnoId: string, req: SubmitEvaluacionRequest): Observable<void> {
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.EVALUACIONES.BASE.replace('{id}', alumnoId)}`,
      req
    );
  }
}
