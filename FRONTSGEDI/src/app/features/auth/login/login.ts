import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../../core/services/auth.service';
import { MessageService } from 'primeng/api';
import { NgOptimizedImage } from '@angular/common';

@Component({
    selector: 'app-login',
    imports: [
        ReactiveFormsModule,
        NgOptimizedImage,
        CardModule,
        InputTextModule,
        PasswordModule,
        ButtonModule,
    ],
    templateUrl: './login.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
    private readonly fb = inject(NonNullableFormBuilder);
    private readonly authService = inject(AuthService);
    private readonly router = inject(Router);
    private readonly toast = inject(MessageService);

    readonly loginForm = this.fb.group({
        email: ['', [Validators.required, Validators.email]],
        password: ['', Validators.required],
    });

    readonly isLoading = signal(false);

    onSubmit(): void {
        if (this.loginForm.invalid) return;

        this.isLoading.set(true);
        const credentials = this.loginForm.getRawValue();

        this.authService.login(credentials).subscribe({
            next: (success) => {
                this.isLoading.set(false);
                if (success) {
                    this.router.navigate(['/dashboard']);
                }
            },
            error: (err) => {
                this.isLoading.set(false);
                this.toast.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: err.error?.detail ?? 'Credenciales inválidas'
                });
            },
        });
    }

    registerStudent(): void {
        this.router.navigate(['/register']);
    }
}
