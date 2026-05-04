export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: '/auth/login',
    LOGOUT: '/auth/logout',
    REFRESH: '/auth/refresh-token',
    REGISTER_STUDENT: '/auth/register-student'
  },
  USERS: {
    ME: '/users/me'
  },
  STUDENTS: {
    ME: '/alumnos/me',
    LIST: '/alumnos',
    DETAIL: '/alumnos/{id}',
    ACTIVATE: '/alumnos/{id}/activate',
    SUSPEND: '/alumnos/{id}/suspend',
    CARGA_ACADEMICA: '/alumnos/{id}/carga-academica',
    DOCUMENTS: '/alumnos/{id}/documentos',
    EXPEDIENTE: '/alumnos/{id}/expediente',
    SEMESTRES: '/alumnos/{id}/semestres',
    MY_DEADLINES: '/alumnos/me/fechas-limite'
  },
  DOCUMENTS: {
    DOWNLOAD: '/documentos/{id}/download'
  },
  PLANTILLAS: {
    BASE: '/plantillas',
    DOWNLOAD: '/plantillas/{id}/download'
  },
  ACUERDOS: {
    BASE: '/acuerdos',
    ADMINISTRATIVE_UPLOAD: '/acuerdos/administrative-upload',
    UPLOAD_STUDENT: '/alumnos/me/documentos/acuerdos',
    UPLOAD_PROFESOR: '/profesor/documentos/acuerdos',
    UPLOAD_ASESOR_INTERNO: '/asesor-interno/documentos/acuerdos',
    UPLOAD_ASESOR_EXTERNO: '/asesor-externo/documentos/acuerdos',
    PRORROGA: '/acuerdos/prorroga'
  },
  CATALOGS: {
    MODULES: '/catalogs/modules',
    ROLES: '/catalogs/roles',
    CARRERAS: '/catalogs/carreras',
    EMPRESAS: '/catalogs/empresas',
    MATERIAS: '/catalogs/materias',
    ASESORES_INTERNOS: '/catalogs/asesores-internos',
    ASESORES_EXTERNOS: '/catalogs/asesores-externos',
    PROFESORES: '/catalogs/profesores'
  },
  USER_MANAGEMENT: {
    INTERNAL: '/users/internal',
    INTERNAL_GET: '/users/internal/{id}',
    INTERNAL_TOGGLE_STATUS: '/users/internal/{id}/toggle-status',
    EXTERNAL: '/users/external',
    EXTERNAL_GET: '/users/external/{id}'
  }
};
