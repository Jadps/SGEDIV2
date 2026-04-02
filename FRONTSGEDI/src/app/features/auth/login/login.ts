import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-login',
    imports: [
        ReactiveFormsModule,
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
            error: () => {
                this.isLoading.set(false);
            },
        });
    }

    fillDemoCredentials(): void {
        this.loginForm.patchValue({
            email: 'demo@mail.com',
            password: 'Demo.2026!',
        });
    }
}
