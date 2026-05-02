import { Component, inject, input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { UserManagementService } from '../../../../../core/services/user-management.service';
import { NotificationService } from '../../../../../core/services/notification.service';
import { ExternalUserDto, CreateExternalUserRequest } from '../../../../../core/models/external-user.dto';

@Component({
  selector: 'app-empresa-asesores-tab',
  standalone: true,
  imports: [CommonModule, FormsModule, TableModule, ButtonModule, DialogModule, InputTextModule, TooltipModule],
  templateUrl: './asesores.component.html'
})
export class EmpresaAsesoresTabComponent implements OnInit {
  private readonly userService = inject(UserManagementService);
  private readonly notificationService = inject(NotificationService);

  empresaId = input.required<string>();

  asesores = signal<ExternalUserDto[]>([]);
  loading = signal<boolean>(false);
  displayModal = signal<boolean>(false);
  isNew = signal<boolean>(true);
  
  editingAsesor = signal<Partial<ExternalUserDto>>({});
  tempPassword = '';

  ngOnInit() {
    this.loadAsesores();
  }

  loadAsesores() {
    this.loading.set(true);
    this.userService.getExternalUsers(this.empresaId()).subscribe({
      next: (data) => {
        this.asesores.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openNew() {
    this.editingAsesor.set({
        empresaId: this.empresaId(),
        name: '',
        email: '',
        puesto: '',
        telefonoOficina: ''
    });
    this.tempPassword = '';
    this.isNew.set(true);
    this.displayModal.set(true);
  }

  editAsesor(asesor: ExternalUserDto) {
    this.editingAsesor.set({ ...asesor });
    this.isNew.set(false);
    this.displayModal.set(true);
  }

  save() {
    if (this.isNew()) {
      const request: CreateExternalUserRequest = {
        name: this.editingAsesor().name!,
        email: this.editingAsesor().email!,
        password: this.tempPassword,
        empresaId: this.empresaId(),
        puesto: this.editingAsesor().puesto!,
        telefonoOficina: this.editingAsesor().telefonoOficina!
      };
      this.userService.createExternalUser(request).subscribe({
        next: () => {
          this.notificationService.success('Asesor creado', 'El asesor se ha registrado correctamente');
          this.displayModal.set(false);
          this.loadAsesores();
        }
      });
    } else {
      this.userService.updateExternalUser(this.editingAsesor() as ExternalUserDto).subscribe({
        next: () => {
          this.notificationService.success('Asesor actualizado', 'Los datos se han actualizado correctamente');
          this.displayModal.set(false);
          this.loadAsesores();
        }
      });
    }
  }

  deleteAsesor(id: string) {
    if (confirm('¿Estás seguro de eliminar este asesor?')) {
      this.userService.deleteExternalUser(id).subscribe({
        next: () => {
          this.notificationService.success('Asesor eliminado', 'El registro se ha marcado como inactivo');
          this.loadAsesores();
        }
      });
    }
  }
}
