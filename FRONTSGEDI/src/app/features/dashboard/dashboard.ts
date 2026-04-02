import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ToastModule } from 'primeng/toast';
import { MessageModule } from 'primeng/message';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ButtonModule, ProgressSpinnerModule, ToastModule, MessageModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent {
  public readonly authService = inject(AuthService);

  public get isAdmin(): boolean {
    const roles = this.authService.currentUser()?.roles;
    if (!roles) return false;
    return roles.some(r => r.name === 'Admin' || r.name === 'TenantAdmin');
  }

}
