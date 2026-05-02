import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../../core/services/catalog.service';
import { CarreraDto } from '../../../core/models/carrera.dto';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { NotificationService } from '../../../core/services/notification.service';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TooltipModule } from 'primeng/tooltip';
import { CarreraDetailModalComponent } from './detail/detail.component';

@Component({
  selector: 'app-carreras',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DialogModule,
    IconFieldModule,
    InputIconModule,
    TooltipModule,
    CarreraDetailModalComponent
  ],
  templateUrl: './carreras.component.html'
})
export class CarrerasComponent implements OnInit {
  private readonly catalogService = inject(CatalogService);
  private readonly notificationService = inject(NotificationService);

  carreras = signal<CarreraDto[]>([]);
  loading = signal<boolean>(false);
  searchTerm = signal<string>('');

  displayDetail = signal<boolean>(false);
  selectedCarreraId = signal<number | null>(null);

  filteredCarreras = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.carreras();
    return this.carreras().filter(c =>
      c.nombre.toLowerCase().includes(term) ||
      c.clave.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.loadCarreras();
  }

  loadCarreras() {
    this.loading.set(true);
    this.catalogService.getCarreras().subscribe({
      next: (data) => {
        this.carreras.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openNew() {
    this.selectedCarreraId.set(null);
    this.displayDetail.set(true);
  }

  editCarrera(carrera: CarreraDto) {
    this.selectedCarreraId.set(carrera.id);
    this.displayDetail.set(true);
  }

  deleteCarrera(id: number) {
    if (confirm('¿Estás seguro de eliminar esta carrera?')) {
      this.catalogService.deleteCarrera(id).subscribe({
        next: () => {
          this.notificationService.success('Carrera eliminada', 'La carrera se ha eliminado correctamente');
          this.loadCarreras();
        }
      });
    }
  }

  onSearch(event: any) {
    this.searchTerm.set(event.target.value);
  }
}
