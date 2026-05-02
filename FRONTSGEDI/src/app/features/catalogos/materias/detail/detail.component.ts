import { Component, inject, input, output, signal, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CatalogService } from '../../../../core/services/catalog.service';
import { MateriaDto } from '../../../../core/models/materia.dto';
import { CarreraDto } from '../../../../core/models/carrera.dto';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-materia-detail-modal',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    DialogModule, 
    InputTextModule, 
    InputNumberModule,
    SelectModule,
    ButtonModule, 
    ProgressSpinnerModule
  ],
  templateUrl: './detail.component.html'
})
export class MateriaDetailModalComponent {
  private readonly catalogService = inject(CatalogService);
  private readonly notificationService = inject(NotificationService);

  materiaId = input<string | null>(null);
  visible = input.required<boolean>();
  onClose = output<void>();
  onSaved = output<void>();

  materia = signal<Partial<MateriaDto> | null>(null);
  carreras = signal<CarreraDto[]>([]);
  loading = signal(false);

  constructor() {
    this.loadCarreras();

    effect(() => {
      const isVisible = this.visible();
      const id = this.materiaId();

      if (isVisible && id) {
        untracked(() => this.fetch());
      } else if (isVisible && !id) {
        untracked(() => {
          this.materia.set({
            id: '',
            clave: '',
            nombre: '',
            creditos: 0,
            semestre: 1,
            carreraId: 0
          });
        });
      }
    });
  }

  private loadCarreras() {
    this.catalogService.getCarreras().subscribe(data => this.carreras.set(data));
  }

  private fetch() {
    this.loading.set(true);
    this.catalogService.getMaterias().subscribe({
      next: (data) => {
        const found = data.find(m => m.id === this.materiaId());
        if (found) {
          this.materia.set({ ...found });
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  save() {
    const data = this.materia();
    if (!data) return;

    if (!this.materiaId()) {
      this.catalogService.createMateria(data).subscribe({
        next: () => {
          this.notificationService.success('Materia creada', 'La materia se ha registrado correctamente');
          this.onSaved.emit();
          this.onClose.emit();
        }
      });
    } else {
      this.catalogService.updateMateria(data as MateriaDto).subscribe({
        next: () => {
          this.notificationService.success('Materia actualizada', 'Los datos se han actualizado correctamente');
          this.onSaved.emit();
          this.onClose.emit();
        }
      });
    }
  }
}
