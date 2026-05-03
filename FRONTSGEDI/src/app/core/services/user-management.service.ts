import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { InternalUserDto, CreateInternalUserRequest } from '../models/internal-user.dto';
import { ExternalUserDto, CreateExternalUserRequest } from '../models/external-user.dto';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private readonly http = inject(HttpClient);

  getInternalUsers(): Observable<InternalUserDto[]> {
    return this.http.get<InternalUserDto[]>(`${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.INTERNAL}`);
  }

  getInternalUser(id: string): Observable<InternalUserDto> {
    const url = `${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.INTERNAL_GET}`.replace('{id}', id);
    return this.http.get<InternalUserDto>(url);
  }

  createInternalUser(request: CreateInternalUserRequest): Observable<InternalUserDto> {
    return this.http.post<InternalUserDto>(`${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.INTERNAL}`, request);
  }

  updateInternalUser(request: InternalUserDto): Observable<InternalUserDto> {
    return this.http.put<InternalUserDto>(`${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.INTERNAL}`, request);
  }

  toggleInternalUserStatus(id: string): Observable<boolean> {
    const url = `${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.INTERNAL_TOGGLE_STATUS}`.replace('{id}', id);
    return this.http.patch<boolean>(url, {});
  }

  getExternalUsers(empresaId?: string): Observable<ExternalUserDto[]> {
    let params = {};
    if (empresaId) {
      params = { empresaId };
    }
    return this.http.get<ExternalUserDto[]>(`${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.EXTERNAL}`, { params });
  }

  getExternalUser(id: string): Observable<ExternalUserDto> {
    const url = `${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.EXTERNAL_GET}`.replace('{id}', id);
    return this.http.get<ExternalUserDto>(url);
  }

  createExternalUser(request: CreateExternalUserRequest): Observable<ExternalUserDto> {
    return this.http.post<ExternalUserDto>(`${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.EXTERNAL}`, request);
  }

  updateExternalUser(request: ExternalUserDto): Observable<ExternalUserDto> {
    return this.http.put<ExternalUserDto>(`${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.EXTERNAL}`, request);
  }

  deleteExternalUser(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}${API_ENDPOINTS.USER_MANAGEMENT.EXTERNAL}/${id}`);
  }
}
