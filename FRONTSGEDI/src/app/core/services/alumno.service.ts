import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { AlumnoDto } from '../models/alumno.dto';
import { AlumnoDetailDto } from '../models/alumno-detail.dto';
import { PagedResponse } from '../models/paged-response.dto';
import { MyAlumnoProfileDto } from '../models/my-alumno-profile.dto';

@Injectable({ providedIn: 'root' })
export class AlumnoService {
  private readonly http = inject(HttpClient);

  getAlumnos(
    page: number = 1,
    pageSize: number = 10,
    searchTerm?: string,
    carreraId?: number
  ): Observable<PagedResponse<AlumnoDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (searchTerm) params = params.set('searchTerm', searchTerm);
    if (carreraId) params = params.set('carreraId', carreraId.toString());

    return this.http.get<PagedResponse<AlumnoDto>>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.LIST}`,
      { params }
    );
  }

  getAlumno(id: string): Observable<AlumnoDetailDto> {
    return this.http.get<AlumnoDetailDto>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.DETAIL.replace('{id}', id)}`
    );
  }

  getMyProfile(): Observable<MyAlumnoProfileDto> {
    return this.http.get<MyAlumnoProfileDto>(`${environment.apiUrl}${API_ENDPOINTS.STUDENTS.ME}`);
  }

  suspendStudent(id: string): Observable<void> {
    return this.http.patch<void>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.SUSPEND.replace('{id}', id)}`,
      {}
    );
  }

  activate(payload: { alumnoId: string; empresaId: string; asesorInternoId: string; asesorExternoId: string }): Observable<void> {
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.ACTIVATE.replace('{id}', payload.alumnoId)}`,
      payload
    );
  }

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
