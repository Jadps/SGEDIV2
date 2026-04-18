import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { AlumnoDto } from '../models/alumno.dto';
import { AlumnoDetailDto } from '../models/alumno-detail.dto';
import { PagedResponse } from '../models/paged-response.dto';

@Injectable({
  providedIn: 'root'
})
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

    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }

    if (carreraId) {
      params = params.set('carreraId', carreraId.toString());
    }

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

  toggleStatus(id: string): Observable<{ isActive: boolean }> {
    return this.http.patch<{ isActive: boolean }>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.DETAIL.replace('{id}', id)}/toggle-status`,
      {}
    );
  }
}
