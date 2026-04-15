import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const user = authService.currentUser();
  const allowedRoles = route.data['roles'] as string[];

  if (!user) {
    router.navigate(['/login']);
    return false;
  }

  const hasRole = user.roles.some(role => allowedRoles.includes(role.name));

  if (hasRole) {
    return true;
  }

  console.warn(`Access denied for URL: ${state.url}. User does not have required roles: ${allowedRoles.join(', ')}`);
  router.navigate(['/dashboard']);
  return false;
};
