import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, map, Observable, of, switchMap, tap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { UserDto } from '../models/user.dto';
import { LoginDto } from '../models/auth.dto';
import { MenuService } from './menu.service';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private readonly AUTH_STATUS_KEY = 'auth_status';
    public currentUser = signal<UserDto | null>(null);
    public isRefreshing = signal<boolean>(false);
    public refreshTokenSubject = new BehaviorSubject<boolean | null>(null);
    private readonly http = inject(HttpClient);
    private readonly menuService = inject(MenuService);

    isAuthenticatedHint(): boolean {
        return localStorage.getItem(this.AUTH_STATUS_KEY) === 'loggedIn';
    }

    login(credentials: LoginDto): Observable<boolean> {
        return this.http.post<any>(`${environment.apiUrl}${API_ENDPOINTS.AUTH.LOGIN}`, credentials).pipe(
            tap(() => {
                localStorage.setItem(this.AUTH_STATUS_KEY, 'loggedIn');
            }),
            switchMap(() => {
                return this.getProfile();
            }),
            catchError((err) => {
                this.clearSession();
                return throwError(() => err);
            })
        );
    }

    refreshToken(): Observable<boolean> {
        return this.http.post<any>(`${environment.apiUrl}${API_ENDPOINTS.AUTH.REFRESH}`, {}).pipe(
            map(() => true),
            catchError(() => {
                this.clearSession();
                return of(false);
            })
        );
    }

    logout(): Observable<boolean> {
        return this.http.post(`${environment.apiUrl}${API_ENDPOINTS.AUTH.LOGOUT}`, {}).pipe(
            tap(() => {
                this.clearSession();
            }),
            map(() => true),
            catchError(() => {
                this.clearSession();
                return of(false);
            })
        );
    }

    clearSession(): void {
        localStorage.setItem(this.AUTH_STATUS_KEY, 'loggedOut');
        this.currentUser.set(null);
        this.menuService.invalidate();
    }

    getProfile(): Observable<boolean> {
        return this.http.get<UserDto>(`${environment.apiUrl}${API_ENDPOINTS.USERS.ME}`).pipe(
            tap((user) => {
                this.currentUser.set(user);
            }),
            map(() => true),
            catchError(() => {
                this.clearSession();
                return of(false);
            })
        );
    }
}