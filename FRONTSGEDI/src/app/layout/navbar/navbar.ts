import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { NgOptimizedImage } from '@angular/common';
@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [ButtonModule, TooltipModule, NgOptimizedImage],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  public authService = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/login']);
    });
  }
}
