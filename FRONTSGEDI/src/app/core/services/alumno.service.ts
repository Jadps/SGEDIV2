import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpContext } from '@angular/common/http';

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

  getMyProfile(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/alumnos/me`);
  }

  toggleStatus(id: string): Observable<{ isActive: boolean }> {
    return this.http.patch<{ isActive: boolean }>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.DETAIL.replace('{id}', id)}/toggle-status`,
      {}
    );
  }

  getExpediente(alumnoId: string, semestre?: string): Observable<any[]> {
    let params = new HttpParams();
    if (semestre) {
      params = params.set('semestre', semestre);
    }
    return this.http.get<any[]>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.EXPEDIENTE.replace('{id}', alumnoId)}`,
      { params }
    );
  }

  getSemestres(alumnoId: string): Observable<string[]> {
    return this.http.get<string[]>(
      `${environment.apiUrl}/alumnos/${alumnoId}/semestres`
    );
  }

  reviewDocument(alumnoId: string, docId: string, aprobado: boolean, motivoRechazo?: string): Observable<void> {
    return this.http.patch<void>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.DOCUMENTS.replace('{id}', alumnoId)}/${docId}/revisar`,
      { aprobado, motivoRechazo }
    );
  }

  getTemplates(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.BASE}`);
  }

  downloadTemplate(id: number, context?: HttpContext): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.DOWNLOAD.replace('{id}', id.toString())}`, {
      responseType: 'blob',
      context
    });
  }



  downloadDocument(id: string, context?: HttpContext): Observable<Blob> {
    return this.http.get(`${environment.apiUrl}/documentos/${id}/download`, {
      responseType: 'blob',
      context
    });
  }


  extendDeadline(payload: { acuerdoId?: string, alumnoId?: string, tipoAcuerdo?: number, semestre?: string, nuevaFechaLimite: Date }): Observable<void> {
    return this.http.patch<void>(
      `${environment.apiUrl}/acuerdos/prorroga`,
      { ...payload, nuevaFechaLimite: payload.nuevaFechaLimite.toISOString() }
    );
  }

  uploadAdministrativeAcuerdo(acuerdoId: string, file: File): Observable<void> {
    const formData = new FormData();
    formData.append('AcuerdoId', acuerdoId);
    formData.append('File', file);
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.ACUERDOS.UPLOAD.replace('{id}', acuerdoId)}`,
      formData
    );
  }

  uploadStudentAcuerdo(tipoAcuerdo: number, file: File, context?: HttpContext): Observable<void> {
    const formData = new FormData();
    formData.append('TipoAcuerdo', tipoAcuerdo.toString());
    formData.append('File', file);
    return this.http.post<void>(`${environment.apiUrl}/alumnos/me/documentos/acuerdos`, formData, { context });
  }

  uploadAdministrativePersonalDoc(alumnoId: string, tipoDocumento: number, file: File, context?: HttpContext): Observable<void> {
    const formData = new FormData();
    formData.append('AlumnoId', alumnoId);
    formData.append('TipoDocumento', tipoDocumento.toString());
    formData.append('File', file);
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.STUDENTS.DOCUMENTS.replace('{id}', alumnoId)}`,
      formData,
      { context }
    );
  }


  getMyDeadlines(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/alumnos/me/fechas-limite`);
  }
}
