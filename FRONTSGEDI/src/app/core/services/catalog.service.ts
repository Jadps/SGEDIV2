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

  getAsesoresInternos(): Observable<AsesorInternoCatalogDto[]> {
    return this.http.get<AsesorInternoCatalogDto[]>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.ASESORES_INTERNOS}`);
  }

  getAsesoresExternos(empresaId?: string): Observable<AsesorExternoCatalogDto[]> {
    const url = `${environment.apiUrl}${API_ENDPOINTS.CATALOGS.ASESORES_EXTERNOS}`;
    return empresaId
      ? this.http.get<AsesorExternoCatalogDto[]>(url, { params: { empresaId } })
      : this.http.get<AsesorExternoCatalogDto[]>(url);
  }

  getProfesores(carreraId?: number): Observable<ProfesorCatalogDto[]> {
    const url = `${environment.apiUrl}${API_ENDPOINTS.CATALOGS.PROFESORES}`;
    return carreraId
      ? this.http.get<ProfesorCatalogDto[]>(url, { params: { carreraId: carreraId.toString() } })
      : this.http.get<ProfesorCatalogDto[]>(url);
  }
}

export interface AsesorInternoCatalogDto {
  profileId: string;
  usuarioId: string;
  name: string;
  email: string;
  numeroEmpleado?: string;
  cubiculo?: string;
}

export interface AsesorExternoCatalogDto {
  profileId: string;
  usuarioId: string;
  name: string;
  email: string;
  empresaNombre: string;
  puesto: string;
}

export interface ProfesorCatalogDto {
  profileId: string;
  usuarioId: string;
  name: string;
  email: string;
  carreraId?: number;
  carreraNombre?: string;
  numeroEmpleado?: string;
}
