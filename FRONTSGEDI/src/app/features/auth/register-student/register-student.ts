import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { FileUploadModule, FileSelectEvent } from 'primeng/fileupload';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { API_ENDPOINTS } from '../../../core/constants/api-endpoints';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-register-student',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        RouterLink,
        CardModule,
        InputTextModule,
        PasswordModule,
        ButtonModule,
        SelectModule,
        FileUploadModule
    ],
    templateUrl: './register-student.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterStudentComponent {
    private readonly fb = inject(NonNullableFormBuilder);
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);
    private readonly http = inject(HttpClient);
    private readonly messageService = inject(MessageService);

    readonly careers = [
        { name: 'Ingeniería en Sistemas', id: 1 },
        { name: 'Ingeniería Industrial', id: 2 },
        { name: 'Licenciatura en Administración', id: 3 },
        { name: 'Ingeniería Mecatrónica', id: 4 },
    ];

    readonly registerForm = this.fb.group({
        name: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]],
        matricula: ['', Validators.required],
        carreraId: [null as number | null, Validators.required],
    });

    horarioFile: File | null = null;
    anexo1File: File | null = null;
    kardexFile: File | null = null;

    readonly isLoading = signal(false);

    onFileSelect(event: FileSelectEvent, type: 'horario' | 'anexo' | 'kardex'): void {
        const file = event.files[0];
        if (type === 'horario') this.horarioFile = file;
        else if (type === 'anexo') this.anexo1File = file;
        else if (type === 'kardex') this.kardexFile = file;
    }

    onSubmit(): void {
        if (this.registerForm.invalid || !this.horarioFile || !this.anexo1File || !this.kardexFile) {
            this.messageService.add({ 
                severity: 'warn', 
                summary: 'Faltan datos', 
                detail: 'Por favor completa todos los campos y sube los 3 archivos PDF requeridos.' 
            });
            return;
        }

        this.isLoading.set(true);
        const values = this.registerForm.getRawValue();
        
        const formData = new FormData();
        formData.append('name', values.name);
        formData.append('email', values.email);
        formData.append('password', values.password);
        formData.append('matricula', values.matricula);
        formData.append('carreraId', values.carreraId!.toString());
        
        formData.append('horarioFile', this.horarioFile);
        formData.append('anexo1File', this.anexo1File);
        formData.append('kardexFile', this.kardexFile);

        this.http.post(`${environment.apiUrl}${API_ENDPOINTS.AUTH.REGISTER_STUDENT}`, formData)
            .subscribe({
                next: () => {
                    this.isLoading.set(false);
                    this.messageService.add({ 
                        severity: 'success', 
                        summary: 'Registro exitoso', 
                        detail: 'Tu cuenta ha sido creada. Ahora puedes iniciar sesión.' 
                    });
                    this.router.navigate(['/auth/login']);
                },
                error: (err) => {
                    this.isLoading.set(false);
                    this.messageService.add({ 
                        severity: 'error', 
                        summary: 'Error', 
                        detail: err.error?.message || 'Ocurrió un error inesperado al registrar.' 
                    });
                }
            });
    }
}
