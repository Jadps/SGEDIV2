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
    DETAIL: '/alumnos/{id}'
  },
  CATALOGS: {
    MODULES: '/catalogs/modules',
    ROLES: '/catalogs/roles',
    CARRERAS: '/catalogs/carreras'
  }
};
