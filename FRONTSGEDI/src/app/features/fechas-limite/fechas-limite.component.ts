import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { FechasLimiteService, FechaLimiteDto } from '../../core/services/fechas-limite.service';
import { CatalogService } from '../../core/services/catalog.service';
import { TagModule } from 'primeng/tag';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-fechas-limite',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, SelectModule, FormsModule, TagModule, ToastModule],
  templateUrl: './fechas-limite.component.html',
  styleUrl: './fechas-limite.component.css'
})
export class FechasLimiteComponent implements OnInit {
  private readonly fechasLimiteService = inject(FechasLimiteService);
  private readonly catalogService = inject(CatalogService);
  private readonly messageService = inject(MessageService);

  carreras = signal<any[]>([]);
  fechas = signal<any[]>([]);

  loading = signal(false);
  saving = signal(false);

  selectedCarrera: number | null = null;
  hasChanges = signal(false);

  ngOnInit() {
    this.catalogService.getCarreras().subscribe(data => this.carreras.set(data));
  }

  loadFechas() {
    if (!this.selectedCarrera) return;
    this.loading.set(true);
    this.hasChanges.set(false);

    this.fechasLimiteService.getFechasLimite(this.selectedCarrera).subscribe({
      next: (data: FechaLimiteDto[]) => {
        const mapped = data.map(d => {
          const date = new Date(d.fechaLimite);
          const year = date.getFullYear();
          const month = String(date.getMonth() + 1).padStart(2, '0');
          const day = String(date.getDate()).padStart(2, '0');
          const hours = String(date.getHours()).padStart(2, '0');
          const minutes = String(date.getMinutes()).padStart(2, '0');

          return {
            ...d,
            fechaLimiteStr: `${year}-${month}-${day}T${hours}:${minutes}`,
            changed: false
          };
        });
        this.fechas.set(mapped);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  markAsChanged() {
    this.hasChanges.set(true);
  }

  saveChanges() {
    if (!this.selectedCarrera) return;

    this.saving.set(true);

    const request = {
      carreraId: this.selectedCarrera,
      fechas: this.fechas().map(f => ({
        tipoAcuerdo: f.tipoAcuerdo,
        fechaLimite: new Date(f.fechaLimiteStr).toISOString()
      }))
    };

    this.fechasLimiteService.updateFechasLimite(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Configuración guardada correctamente' });
        this.loadFechas();
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al guardar las configuraciones: ' + (err.error?.message || '') });
      }
    });
  }
}
