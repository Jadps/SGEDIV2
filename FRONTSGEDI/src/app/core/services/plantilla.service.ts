import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';

@Injectable({
  providedIn: 'root'
})
export class PlantillaService {
  private readonly http = inject(HttpClient);

  getTemplates(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.BASE}`);
  }

  getTemplateTypes(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.BASE}/tipos`);
  }

  uploadTemplate(tipoDocumento: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('TipoDocumento', tipoDocumento.toString());
    formData.append('File', file);

    return this.http.post<any>(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.BASE}`, formData);
  }

  downloadTemplate(id: number): void {
    const url = `${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.DOWNLOAD.replace('{id}', id.toString())}`;
    window.open(url, '_blank');
  }

  updateTemplate(id: number, file: File): Observable<void> {
    const formData = new FormData();
    formData.append('Id', id.toString());
    formData.append('File', file);

    return this.http.patch<void>(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.BASE}/${id}`, formData);
  }

  deleteTemplate(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}${API_ENDPOINTS.PLANTILLAS.BASE}/${id}`);
  }
}
