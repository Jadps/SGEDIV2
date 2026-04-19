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
    DOCUMENTS: '/alumnos/{id}/documentos'
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
    CARRERAS: '/catalogs/carreras'
  }
};
