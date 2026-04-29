import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { MenuService } from '../services/menu.service';
import { map, Observable, of } from 'rxjs';

export const permissionGuard: CanActivateFn = (route, state): Observable<boolean> => {
    const router = inject(Router);
    const menuService = inject(MenuService);
    const targetUrl = state.url.split('?')[0];

    if (targetUrl === '/' || targetUrl === '/dashboard') {
        return of(true);
    }

    return menuService.loadMenu().pipe(
        map(() => {
            const allowedUrls = menuService.getAllowedUrls();

            const isAllowed = allowedUrls.some(url => {
                const normalizedUrl = url.startsWith('/') ? url : `/${url}`;
                return targetUrl === normalizedUrl || targetUrl.startsWith(normalizedUrl + '/');
            });

            if (isAllowed) {
                return true;
            }

            console.warn(`Access denied for URL: ${targetUrl}. Not in allowed modules.`);
            router.navigate(['/dashboard']);
            return false;
        })
    );
};
