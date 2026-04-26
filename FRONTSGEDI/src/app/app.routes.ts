import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { roleGuard } from './core/guards/role.guard';

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
                loadComponent: () => import('./features/dashboard/dashboard').then(m => m.DashboardComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'dashboard/alumnos',
                title: 'Lista de Alumnos | SGEDI',
                loadComponent: () => import('./features/alumnos/list/list.component').then(m => m.AlumnoListComponent),
                canActivate: [permissionGuard, roleGuard],
                data: { roles: ['Admin', 'Profesor', 'Coordinador', 'JefeDepartamento', 'AsesorInterno', 'AsesorExterno'] }
            },
            {
                path: 'dashboard/anexos',
                title: 'Plantillas de Anexos | SGEDI',
                loadComponent: () => import('./features/anexos/anexos.component').then(m => m.AnexosComponent),
                canActivate: [permissionGuard, roleGuard],
                data: { roles: ['Admin', 'Coordinador'] }
            },
            {
                path: 'dashboard/fechas-limite',
                title: 'Fechas Límite | SGEDI',
                loadComponent: () => import('./features/fechas-limite/fechas-limite.component').then(m => m.FechasLimiteComponent),
                canActivate: [permissionGuard, roleGuard],
                data: { roles: ['Admin', 'Coordinador'] }
            },
            {
                path: 'dashboard/mis-documentos',
                title: 'Mis Documentos | SGEDI',
                loadComponent: () => import('./features/mis-documentos/mis-documentos.component').then(m => m.MisDocumentosComponent),
                canActivate: [permissionGuard, roleGuard],
                data: { roles: ['Alumno'] }
            },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    },
    { path: '**', redirectTo: 'login' }
];