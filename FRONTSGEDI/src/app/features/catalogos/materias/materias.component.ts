import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../../core/services/catalog.service';
import { MateriaDto } from '../../../core/models/materia.dto';
import { CarreraDto } from '../../../core/models/carrera.dto';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';
import { NotificationService } from '../../../core/services/notification.service';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TooltipModule } from 'primeng/tooltip';
import { MateriaDetailModalComponent } from './detail/detail.component';

@Component({
  selector: 'app-materias',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TableModule, 
    ButtonModule, 
    InputTextModule, 
    SelectModule, 
    DialogModule,
    IconFieldModule,
    InputIconModule,
    TooltipModule,
    MateriaDetailModalComponent
  ],
  templateUrl: './materias.component.html'
})
export class MateriasComponent implements OnInit {
  private readonly catalogService = inject(CatalogService);
  private readonly notificationService = inject(NotificationService);

  materias = signal<MateriaDto[]>([]);
  carreras = signal<CarreraDto[]>([]);
  selectedCarreraId = signal<number | null>(null);
  
  loading = signal<boolean>(false);
  searchTerm = signal<string>('');
  
  displayDetail = signal<boolean>(false);
  selectedMateriaId = signal<string | null>(null);

  filteredMaterias = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.materias();
    return this.materias().filter(m => 
      m.nombre.toLowerCase().includes(term) || 
      m.clave.toLowerCase().includes(term) ||
      m.carreraNombre?.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.loadCarreras();
    this.loadMaterias();
  }

  loadCarreras() {
    this.catalogService.getCarreras().subscribe(data => {
      this.carreras.set(data);
      if (data.length === 1) {
        this.selectedCarreraId.set(data[0].id);
        this.loadMaterias();
      }
    });
  }

  loadMaterias() {
    this.loading.set(true);
    this.catalogService.getMaterias(this.selectedCarreraId() ?? undefined).subscribe({
      next: (data) => {
        this.materias.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onCarreraFilterChange() {
    this.loadMaterias();
  }

  openNew() {
    this.selectedMateriaId.set(null);
    this.displayDetail.set(true);
  }

  editMateria(materia: MateriaDto) {
    this.selectedMateriaId.set(materia.id);
    this.displayDetail.set(true);
  }

  deleteMateria(id: string) {
    if (confirm('¿Estás seguro de eliminar esta materia?')) {
      this.catalogService.deleteMateria(id).subscribe({
        next: () => {
          this.notificationService.success('Materia eliminada', 'La materia se ha eliminado correctamente');
          this.loadMaterias();
        }
      });
    }
  }

  onSearch(event: any) {
    this.searchTerm.set(event.target.value);
  }
}
