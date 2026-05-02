import { Component, inject, input, output, signal, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CatalogService } from '../../../../core/services/catalog.service';
import { CarreraDto } from '../../../../core/models/carrera.dto';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-carrera-detail-modal',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    DialogModule, 
    InputTextModule, 
    ButtonModule, 
    ProgressSpinnerModule
  ],
  templateUrl: './detail.component.html'
})
export class CarreraDetailModalComponent {
  private readonly catalogService = inject(CatalogService);
  private readonly notificationService = inject(NotificationService);

  carreraId = input<number | null>(null);
  visible = input.required<boolean>();
  onClose = output<void>();
  onSaved = output<void>();

  carrera = signal<Partial<CarreraDto> | null>(null);
  loading = signal(false);

  constructor() {
    effect(() => {
      const isVisible = this.visible();
      const id = this.carreraId();

      if (isVisible && id) {
        untracked(() => this.fetch());
      } else if (isVisible && !id) {
        untracked(() => {
          this.carrera.set({
            id: 0,
            clave: '',
            nombre: ''
          });
        });
      }
    });
  }

  private fetch() {
    this.loading.set(true);
    this.catalogService.getCarreras().subscribe({
      next: (data) => {
        const found = data.find(c => c.id === this.carreraId());
        if (found) {
          this.carrera.set({ ...found });
        }
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  save() {
    const data = this.carrera();
    if (!data) return;

    if (!this.carreraId()) {
      this.catalogService.createCarrera(data).subscribe({
        next: () => {
          this.notificationService.success('Carrera creada', 'La carrera se ha registrado correctamente');
          this.onSaved.emit();
          this.onClose.emit();
        }
      });
    } else {
      this.catalogService.updateCarrera(data as CarreraDto).subscribe({
        next: () => {
          this.notificationService.success('Carrera actualizada', 'Los datos se han actualizado correctamente');
          this.onSaved.emit();
          this.onClose.emit();
        }
      });
    }
  }
}
