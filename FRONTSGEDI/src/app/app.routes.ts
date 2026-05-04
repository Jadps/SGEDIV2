import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
    {
        path: 'login',
        title: 'Iniciar Sesión | SGEDI',
        loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
    },
    {
        path: 'register',
        title: 'Registro de Estudiante | SGEDI',
        loadComponent: () => import('./features/auth/register-student/register-student.component').then(m => m.RegisterStudentComponent)
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
                path: 'alumnos',
                title: 'Lista de Alumnos | SGEDI',
                loadComponent: () => import('./features/alumnos/list/list.component').then(m => m.AlumnoListComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'anexos',
                title: 'Plantillas de Anexos | SGEDI',
                loadComponent: () => import('./features/anexos/anexos.component').then(m => m.AnexosComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'fechas-limite',
                title: 'Fechas Límite | SGEDI',
                loadComponent: () => import('./features/fechas-limite/fechas-limite.component').then(m => m.FechasLimiteComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'mis-documentos',
                title: 'Mis Documentos | SGEDI',
                loadComponent: () => import('./features/mis-documentos/mis-documentos.component').then(m => m.MisDocumentosComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'catalogos/carreras',
                title: 'Carreras | SGEDI',
                loadComponent: () => import('./features/catalogos/carreras/carreras.component').then(m => m.CarrerasComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'catalogos/empresas',
                title: 'Empresas | SGEDI',
                loadComponent: () => import('./features/catalogos/empresas/empresas.component').then(m => m.EmpresasComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'catalogos/materias',
                title: 'Materias | SGEDI',
                loadComponent: () => import('./features/catalogos/materias/materias.component').then(m => m.MateriasComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'usuarios/internos',
                title: 'Usuarios Internos | SGEDI',
                loadComponent: () => import('./features/usuarios/internos/internos.component').then(m => m.InternosComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'professors',
                title: 'Mis Profesores | SGEDI',
                loadComponent: () => import('./features/professors/professor-list.component').then(m => m.ProfessorListComponent),
                canActivate: [permissionGuard]
            },
            {
                path: 'asesores',
                title: 'Mis Asesores | SGEDI',
                loadComponent: () => import('./features/asesores/asesores.component').then(m => m.MisAsesoresComponent),
                canActivate: [permissionGuard]
            },
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
        ]
    },
    { path: '**', redirectTo: 'login' }
];