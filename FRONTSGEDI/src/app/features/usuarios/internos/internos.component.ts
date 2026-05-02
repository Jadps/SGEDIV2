import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserManagementService } from '../../../core/services/user-management.service';
import { InternalUserDto } from '../../../core/models/internal-user.dto';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { NotificationService } from '../../../core/services/notification.service';
import { InternalUserDetailModalComponent } from './detail/detail.component';

@Component({
  selector: 'app-internos',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    IconFieldModule,
    InputIconModule,
    TooltipModule,
    ConfirmDialogModule,
    InternalUserDetailModalComponent
  ],
  templateUrl: './internos.component.html'
})
export class InternosComponent implements OnInit {
  private readonly userService = inject(UserManagementService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  users = signal<InternalUserDto[]>([]);
  loading = signal<boolean>(false);
  searchTerm = signal<string>('');

  displayDetail = signal<boolean>(false);
  selectedUserId = signal<string | null>(null);

  filteredUsers = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.users();
    return this.users().filter(u =>
      u.name.toLowerCase().includes(term) ||
      u.email.toLowerCase().includes(term) ||
      u.roles.some(r => r.toLowerCase().includes(term)) ||
      u.carreraNombre?.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.loading.set(true);
    this.userService.getInternalUsers().subscribe({
      next: (data) => {
        this.users.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  toggleStatus(user: InternalUserDto) {
    const newStatus = !user.isActive;
    const action = newStatus ? 'activar' : 'desactivar';

    this.confirmationService.confirm({
      message: `¿Estás seguro de que deseas ${action} a este usuario?`,
      header: 'Confirmar Acción',
      icon: 'pi pi-exclamation-triangle',
      rejectLabel: 'Cancelar',
      rejectButtonProps: { label: 'Cancelar', severity: 'secondary', outlined: true },
      acceptLabel: newStatus ? 'Activar' : 'Desactivar',
      acceptButtonProps: { label: newStatus ? 'Activar' : 'Desactivar', severity: newStatus ? 'success' : 'danger' },
      accept: () => {
        const updatedUser = { ...user, isActive: newStatus };
        this.userService.updateInternalUser(updatedUser).subscribe({
          next: () => {
            this.notificationService.success(
              `Usuario ${newStatus ? 'activado' : 'desactivado'}`,
              `El usuario se ha ${newStatus ? 'activado' : 'desactivado'} correctamente`
            );
            this.loadUsers();
          }
        });
      }
    });
  }

  openNew() {
    this.selectedUserId.set(null);
    this.displayDetail.set(true);
  }

  editUser(user: InternalUserDto) {
    this.selectedUserId.set(user.id);
    this.displayDetail.set(true);
  }

  onSearch(event: any) {
    this.searchTerm.set(event.target.value);
  }
}
