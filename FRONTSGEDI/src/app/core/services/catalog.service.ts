import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { ModuleDto } from '../models/module.dto';
import { CarreraDto } from '../models/carrera.dto';
import { EmpresaDto } from '../models/empresa.dto';
import { MateriaDto } from '../models/materia.dto';
import { RoleDto } from '../models/role.dto';

@Injectable({
  providedIn: 'root'
})
export class CatalogService {
  private readonly http = inject(HttpClient);

  getMenuModules(): Observable<ModuleDto[]> {
    return this.http.get<ModuleDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MODULES}`);
  }

  getRoles(): Observable<RoleDto[]> {
    return this.http.get<RoleDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.ROLES}`);
  }

  getCarreras(): Observable<CarreraDto[]> {
    return this.http.get<CarreraDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.CARRERAS}`);
  }

  createCarrera(carrera: Partial<CarreraDto>): Observable<CarreraDto> {
    return this.http.post<CarreraDto>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.CARRERAS}`, carrera);
  }

  updateCarrera(carrera: CarreraDto): Observable<CarreraDto> {
    return this.http.put<CarreraDto>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.CARRERAS}`, carrera);
  }

  deleteCarrera(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.CARRERAS}/${id}`);
  }

  getEmpresas(): Observable<EmpresaDto[]> {
    return this.http.get<EmpresaDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.EMPRESAS}`);
  }

  createEmpresa(empresa: Partial<EmpresaDto>): Observable<EmpresaDto> {
    return this.http.post<EmpresaDto>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.EMPRESAS}`, empresa);
  }

  updateEmpresa(empresa: EmpresaDto): Observable<EmpresaDto> {
    return this.http.put<EmpresaDto>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.EMPRESAS}`, empresa);
  }

  deleteEmpresa(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.EMPRESAS}/${id}`);
  }

  getMaterias(carreraId?: number): Observable<MateriaDto[]> {
    let params = {};
    if (carreraId !== undefined && carreraId !== null) {
      params = { carreraId: carreraId.toString() };
    }
    return this.http.get<MateriaDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MATERIAS}`, { params });
  }

  createMateria(materia: Partial<MateriaDto>): Observable<MateriaDto> {
    return this.http.post<MateriaDto>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MATERIAS}`, materia);
  }

  updateMateria(materia: MateriaDto): Observable<MateriaDto> {
    return this.http.put<MateriaDto>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MATERIAS}`, materia);
  }

  deleteMateria(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MATERIAS}/${id}`);
  }
}
