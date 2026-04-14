import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: 'login',
        title: 'Iniciar Sesión | SGEDI',
        loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
    },
    {
        path: 'register',
        title: 'Registro de Estudiante | SGEDI',
        loadComponent: () => import('./features/auth/register-student/register-student').then(m => m.RegisterStudentComponent)
    },
    {
        path: '',
        loadComponent: () => import('./layout/app.layout').then(m => m.AppLayout),
        canActivate: [authGuard],
        children: [
            {
                path: 'dashboard',
                title: 'Dashboard | SGEDI',
                loadComponent: () => import('./features/dashboard/dashboard').then(m => m.DashboardComponent)
            },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    },
    { path: '**', redirectTo: 'login' }
];