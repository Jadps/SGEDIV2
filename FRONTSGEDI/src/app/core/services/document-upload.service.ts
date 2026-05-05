import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';

@Injectable({ providedIn: 'root' })
export class DocumentUploadService {
  private readonly http = inject(HttpClient);

  uploadStudentAcuerdo(tipoAcuerdo: number, file: File, context?: HttpContext): Observable<void> {
    const formData = new FormData();
    formData.append('TipoAcuerdo', tipoAcuerdo.toString());
    formData.append('File', file);
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.UPLOAD_STUDENT}`,
      formData,
      { context }
    );
  }

  uploadProfesorAcuerdo(alumnoId: string, tipoAcuerdo: number, file: File, materiaId?: string, context?: HttpContext): Observable<void> {
    const formData = new FormData();
    formData.append('AlumnoId', alumnoId);
    formData.append('TipoAcuerdo', tipoAcuerdo.toString());
    if (materiaId) formData.append('MateriaId', materiaId);
    formData.append('File', file);
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.UPLOAD_PROFESOR}`,
      formData,
      { context }
    );
  }

  uploadAsesorInternoAcuerdo(alumnoId: string, tipoAcuerdo: number, file: File, context?: HttpContext): Observable<void> {
    const formData = new FormData();
    formData.append('AlumnoId', alumnoId);
    formData.append('TipoAcuerdo', tipoAcuerdo.toString());
    formData.append('File', file);
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.UPLOAD_ASESOR_INTERNO}`,
      formData,
      { context }
    );
  }

  uploadAsesorExternoAcuerdo(alumnoId: string, tipoAcuerdo: number, file: File, context?: HttpContext): Observable<void> {
    const formData = new FormData();
    formData.append('AlumnoId', alumnoId);
    formData.append('TipoAcuerdo', tipoAcuerdo.toString());
    formData.append('File', file);
    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.UPLOAD_ASESOR_EXTERNO}`,
      formData,
      { context }
    );
  }



  uploadAdministrativeAcuerdo(
    file: File,
    acuerdoId?: string | null,
    alumnoId?: string | null,
    tipoAcuerdo?: number | null,
    profesorId?: string | null
  ): Observable<void> {
    const formData = new FormData();
    if (acuerdoId) formData.append('AcuerdoId', acuerdoId);
    if (alumnoId) formData.append('AlumnoId', alumnoId);
    if (tipoAcuerdo !== undefined && tipoAcuerdo !== null) formData.append('TipoAcuerdo', tipoAcuerdo.toString());
    if (profesorId) formData.append('ProfesorId', profesorId);
    formData.append('File', file);

    return this.http.post<void>(
      `${environment.apiUrl}${API_ENDPOINTS.CONTRATOS.ADMINISTRATIVE_UPLOAD}`,
      formData
    );
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

  downloadDocument(id: string, context?: HttpContext): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}${API_ENDPOINTS.DOCUMENTS.DOWNLOAD.replace('{id}', id)}`,
      { responseType: 'blob', context }
    );
  }

  getTemplates(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.BASE}`);
  }

  downloadTemplate(id: number, context?: HttpContext): Observable<Blob> {
    return this.http.get(
      `${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.DOWNLOAD.replace('{id}', id.toString())}`,
      { responseType: 'blob', context }
    );
  }
}

