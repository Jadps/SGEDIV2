import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class CargaAcademicaService {
  private readonly http = inject(HttpClient);

  getCargaAcademica(alumnoId: string): Observable<any[]> {
    return this.http.get<any[]>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.CARGA_ACADEMICA.replace('{id}', alumnoId)}`
    );
  }

  setCargaAcademica(materias: { materiaId: string; profesorId: string }[]): Observable<void> {
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.CARGA_ACADEMICA.replace('{id}', 'me')}`,
      { materias }
    );
  }
}
