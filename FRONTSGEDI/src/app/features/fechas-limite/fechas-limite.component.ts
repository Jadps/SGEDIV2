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
import { DatePickerModule } from 'primeng/datepicker';


@Component({
  selector: 'app-fechas-limite',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, SelectModule, FormsModule, TagModule, DatePickerModule],
  templateUrl: './fechas-limite.component.html'
})
export class FechasLimiteComponent implements OnInit {
  private readonly fechasLimiteService = inject(FechasLimiteService);
  private readonly catalogService = inject(CatalogService);
  private readonly messageService = inject(MessageService);

  carreras = signal<any[]>([]);
  fechas = signal<any[]>([]);

  loading = signal(false);
  saving = signal(false);

  selectedCarrera = signal<number | null>(null);
  hasChanges = signal(false);

  ngOnInit() {
    this.catalogService.getCarreras().subscribe(data => {
      this.carreras.set(data);
      if (data.length === 1) {
        this.selectedCarrera.set(data[0].id);
        this.loadFechas();
      }
    });
  }

  loadFechas() {
    if (!this.selectedCarrera()) return;
    this.loading.set(true);
    this.hasChanges.set(false);

    this.fechasLimiteService.getFechasLimite(this.selectedCarrera()!).subscribe({
      next: (data: FechaLimiteDto[]) => {
        const mapped = data.map(d => {
          return {
            ...d,
            fechaLimiteDate: new Date(d.fechaLimite),
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
    if (!this.selectedCarrera()) return;

    const invalidDates = this.fechas().filter(f => !f.fechaLimiteDate || isNaN(f.fechaLimiteDate.getTime()));
    if (invalidDates.length > 0) {
      this.messageService.add({ 
        severity: 'error', 
        summary: 'Fechas Inválidas', 
        detail: 'Por favor, asegúrate de que todas las fechas sean válidas.' 
      });
      return;
    }

    this.saving.set(true);

    const request = {
      carreraId: this.selectedCarrera()!,
      fechas: this.fechas().map(f => ({
        tipoAcuerdo: f.tipoAcuerdo,
        fechaLimite: f.fechaLimiteDate.toISOString()
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
