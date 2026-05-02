import { Component, inject, input, output, signal, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { TabsModule } from 'primeng/tabs';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CatalogService } from '../../../../core/services/catalog.service';
import { EmpresaDto } from '../../../../core/models/empresa.dto';
import { EmpresaInfoTabComponent } from '../tabs/info/info.component';
import { EmpresaAsesoresTabComponent } from '../tabs/asesores/asesores.component';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-empresa-detail-modal',
  standalone: true,
  imports: [
    CommonModule, DialogModule, TabsModule,
    ProgressSpinnerModule, EmpresaInfoTabComponent,
    EmpresaAsesoresTabComponent
  ],
  templateUrl: './detail.component.html'
})
export class EmpresaDetailModalComponent {
  private readonly catalogService = inject(CatalogService);
  private readonly notificationService = inject(NotificationService);

  empresaId = input<string | null>(null);
  visible = input.required<boolean>();
  onClose = output<void>();
  onSaved = output<void>();

  empresa = signal<EmpresaDto | null>(null);
  loading = signal(false);
  activeTab = signal<string>('info');

  constructor() {
    effect(() => {
      const isVisible = this.visible();
      const id = this.empresaId();

      if (isVisible && id) {
        untracked(() => this.fetch());
      } else if (isVisible && !id) {
        untracked(() => {
          this.empresa.set({
            id: '',
            nombre: '',
            rfc: '',
            direccion: '',
            telefono: '',
            correo: ''
          });
          this.activeTab.set('info');
        });
      }
    });
  }

  onTabChange(event: string | number | undefined) {
    if (event !== undefined) {
      this.activeTab.set(event as string);
    }
  }

  private fetch() {
    this.loading.set(true);
    this.catalogService.getEmpresas().subscribe({
      next: (data) => {
        const found = data.find(e => e.id === this.empresaId());
        if (found) {
          this.empresa.set(found);
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  saveEmpresa(data: EmpresaDto) {
    if (!this.empresaId()) {
      this.catalogService.createEmpresa(data).subscribe({
        next: () => {
          this.notificationService.success('Empresa creada', 'La empresa se ha registrado correctamente');
          this.onSaved.emit();
          this.onClose.emit();
        }
      });
    } else {
      this.catalogService.updateEmpresa(data).subscribe({
        next: () => {
          this.notificationService.success('Empresa actualizada', 'Los datos se han actualizado correctamente');
          this.onSaved.emit();
        }
      });
    }
  }
}
