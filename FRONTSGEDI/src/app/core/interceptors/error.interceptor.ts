import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { NotificationService } from '../services/notification.service';
import { API_ENDPOINTS } from '../constants/api-endpoints';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const notificationService = inject(NotificationService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401 &&
                !req.url.includes(API_ENDPOINTS.AUTH.LOGIN) &&
                !req.url.includes(API_ENDPOINTS.AUTH.REFRESH) &&
                !req.url.includes(API_ENDPOINTS.AUTH.LOGOUT)) {

                if (!authService.isRefreshing()) {
                    authService.isRefreshing.set(true);
                    authService.refreshTokenSubject.next(null);

                    return authService.refreshToken().pipe(
                        switchMap((success) => {
                            authService.isRefreshing.set(false);

                            if (success) {
                                authService.refreshTokenSubject.next(true);
                                return next(req);
                            } else {
                                authService.clearSession();
                                router.navigate(['/login']);
                                return throwError(() => error);
                            }
                        }),
                        catchError((refreshError) => {
                            authService.isRefreshing.set(false);
                            authService.clearSession();
                            router.navigate(['/login']);
                            return throwError(() => refreshError);
                        })
                    );
                } else {
                    return authService.refreshTokenSubject.pipe(
                        filter(result => result !== null),
                        take(1),
                        switchMap((success) => {
                            if (success) {
                                return next(req);
                            } else {
                                return throwError(() => error);
                            }
                        })
                    );
                }
            } else if (error.status !== 401) {
                let detail = 'Ocurrió un error inesperado. Inténtelo de nuevo.';

                if (error.status === 400) {
                    detail = error.error?.detail || error.error?.message || 'La solicitud es inválida.';
                } else if (error.status === 403) {
                    detail = 'No tiene permisos para realizar esta acción.';
                } else if (error.status === 500) {
                    detail = 'Error interno del servidor. El equipo técnico ha sido notificado.';
                } else if (error.status === 429) {
                    detail = 'Demasiadas solicitudes. Por favor, espere un momento.';
                }

                notificationService.error('Error del Sistema', detail);
            }

            return throwError(() => error);
        })
    );
};