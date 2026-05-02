import { Component, inject, input, output, signal, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { UserManagementService } from '../../../../core/services/user-management.service';
import { CatalogService } from '../../../../core/services/catalog.service';
import { InternalUserDto, CreateInternalUserRequest } from '../../../../core/models/internal-user.dto';
import { CarreraDto } from '../../../../core/models/carrera.dto';
import { RoleDto } from '../../../../core/models/role.dto';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-internal-user-detail-modal',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    DialogModule, 
    InputTextModule, 
    MultiSelectModule,
    SelectModule,
    CheckboxModule,
    ButtonModule, 
    ProgressSpinnerModule
  ],
  templateUrl: './detail.component.html'
})
export class InternalUserDetailModalComponent {
  private readonly userService = inject(UserManagementService);
  private readonly catalogService = inject(CatalogService);
  private readonly notificationService = inject(NotificationService);

  userId = input<string | null>(null);
  visible = input.required<boolean>();
  onClose = output<void>();
  onSaved = output<void>();

  user = signal<Partial<InternalUserDto & { password?: string }> | null>(null);
  carreras = signal<CarreraDto[]>([]);
  availableRoles = signal<RoleDto[]>([]);
  loading = signal(false);

  constructor() {
    this.loadCatalogos();

    effect(() => {
      const isVisible = this.visible();
      const id = this.userId();

      if (isVisible && id) {
        untracked(() => this.fetch());
      } else if (isVisible && !id) {
        untracked(() => {
          this.user.set({
            name: '',
            email: '',
            password: '',
            roles: [],
            isActive: true
          });
        });
      }
    });
  }

  private loadCatalogos() {
    this.catalogService.getCarreras().subscribe(data => this.carreras.set(data));
    this.catalogService.getRoles().subscribe(data => this.availableRoles.set(data));
  }

  private fetch() {
    this.loading.set(true);
    this.userService.getInternalUsers().subscribe({
      next: (data) => {
        const found = data.find(u => u.id === this.userId());
        if (found) {
          this.user.set({ ...found });
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  hasRole(role: string): boolean {
    return this.user()?.roles?.includes(role) ?? false;
  }

  save() {
    const data = this.user();
    if (!data) return;

    if (!this.userId()) {
      this.userService.createInternalUser(data as CreateInternalUserRequest).subscribe({
        next: () => {
          this.notificationService.success('Usuario creado', 'El usuario se ha registrado correctamente');
          this.onSaved.emit();
          this.onClose.emit();
        }
      });
    } else {
      this.userService.updateInternalUser(data as InternalUserDto).subscribe({
        next: () => {
          this.notificationService.success('Usuario actualizado', 'Los datos se han actualizado correctamente');
          this.onSaved.emit();
          this.onClose.emit();
        }
      });
    }
  }
}
