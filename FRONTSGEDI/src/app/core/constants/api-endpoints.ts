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
    LIST: '/alumnos',
    DETAIL: '/alumnos/{id}',
    DOCUMENTS: '/alumnos/{id}/documentos',
    EXPEDIENTE: '/alumnos/{id}/expediente'
  },
  PLANTILLAS: {
    BASE: '/plantillas',
    DOWNLOAD: '/plantillas/{id}/download'
  },
  ACUERDOS: {
    BASE: '/acuerdos',
    UPLOAD: '/acuerdos/{id}/upload'
  },
  CATALOGS: {
    MODULES: '/catalogs/modules',
    ROLES: '/catalogs/roles',
    CARRERAS: '/catalogs/carreras',
    EMPRESAS: '/catalogs/empresas',
    MATERIAS: '/catalogs/materias'
  },
  USER_MANAGEMENT: {
    INTERNAL: '/users/internal',
    INTERNAL_GET: '/users/internal/{id}',
    INTERNAL_TOGGLE_STATUS: '/users/internal/{id}/toggle-status',
    EXTERNAL: '/users/external',
    EXTERNAL_GET: '/users/external/{id}'
  }
};
