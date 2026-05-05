export interface ContratoDto {
  id: string;
  materiaId: string;
  materiaNombre: string;
  alumnoId: string;
  alumnoNombre: string;
  profesorId: string;
  profesorNombre: string;
  modalidad: number;
  modalidadLabel: string;
  descripcion?: string;
  estado: string;
  motivoRechazo?: string;
  fechaCreacion: string;
  criterios: CriterioDto[];
}

export interface CriterioDto {
  id: string;
  tipo: number;
  tipoLabel: string;
  detalle: string;
  porcentaje: number;
}

export interface CreateContratoRequest {
  alumnoId: string;
  materiaId: string;
  modalidad: number;
  descripcion: string;
  criterios: {
    tipo: number;
    detalle: string;
    porcentaje: number;
  }[];
}

export interface ContratoCatalogsDto {
  modalidades: CatalogItemDto[];
  tiposCriterio: CatalogItemDto[];
  estados: CatalogItemDto[];
}

export interface CatalogItemDto {
  value: number;
  label: string;
}
