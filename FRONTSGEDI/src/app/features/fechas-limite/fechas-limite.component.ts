import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { FechasLimiteService, FechaLimiteDto } from '../../core/services/fechas-limite.service';
import { CatalogService } from '../../core/services/catalog.service';
import { TagModule } from 'primeng/tag';

@Component({
  selector: 'app-fechas-limite',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, SelectModule, FormsModule, TagModule],
  template: `
    <div class="p-8 h-full overflow-y-auto">
      <div class="max-w-6xl mx-auto">
        <div class="flex justify-between items-end mb-8">
          <div>
            <h1 class="text-3xl font-bold text-white mb-2">Fechas Límite de Anexos</h1>
            <p class="text-zinc-400">Configura las fechas de entrega máximas globales por carrera.</p>
          </div>
          <p-button label="Guardar Cambios" icon="pi pi-save" (onClick)="saveChanges()" [loading]="saving()" [disabled]="!selectedCarrera || !hasChanges()" />
        </div>

        <div class="flex gap-4 mb-6">
          <div class="flex flex-col gap-2 flex-1 max-w-sm">
            <label class="text-sm font-medium text-zinc-400">Carrera</label>
            <p-select [options]="carreras()" [(ngModel)]="selectedCarrera" (onChange)="loadFechas()" 
              optionLabel="name" optionValue="id" placeholder="Selecciona una carrera" 
              styleClass="w-full bg-zinc-900/50 border-white/10" />
          </div>
        </div>

        <div class="bg-zinc-900/50 border border-white/5 rounded-2xl overflow-hidden backdrop-blur-xl" *ngIf="selectedCarrera">
          <p-table [value]="fechas()" styleClass="p-datatable-sm glass-table" [loading]="loading()">
            <ng-template pTemplate="header">
              <tr>
                <th>Tipo de Anexo</th>
                <th>Fecha Límite Configurada</th>
                <th>Estado</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-fecha>
              <tr class="transition-colors hover:bg-white/5">
                <td class="text-sm font-medium text-zinc-300">Anexo {{ fecha.tipoAcuerdo }}</td>
                <td>
                  <input type="datetime-local" 
                    class="p-2 bg-zinc-800/50 border border-white/10 rounded-xl text-white outline-none focus:border-white/30 w-full max-w-sm"
                    [(ngModel)]="fecha.fechaLimiteStr" (ngModelChange)="markAsChanged()">
                </td>
                <td>
                  <p-tag *ngIf="fecha.isDefault && !fecha.changed" severity="info" value="Default" />
                  <p-tag *ngIf="!fecha.isDefault && !fecha.changed" severity="success" value="Personalizado" />
                  <p-tag *ngIf="fecha.changed" severity="warn" value="Modificado" />
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="3" class="text-center py-12 text-zinc-500">Selecciona una carrera para ver sus fechas límite.</td>
              </tr>
            </ng-template>
          </p-table>
        </div>

        <div *ngIf="!selectedCarrera" class="text-center py-12 bg-zinc-900/30 border border-white/5 rounded-2xl">
          <i class="pi pi-briefcase text-4xl text-zinc-600 mb-4"></i>
          <p class="text-zinc-400">Selecciona una carrera para administrar sus fechas límite.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host ::ng-deep .glass-table .p-datatable-thead > tr > th {
      background: rgba(255, 255, 255, 0.02);
      color: var(--zinc-400);
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      padding: 1rem;
    }
    :host ::ng-deep .glass-table .p-datatable-tbody > tr {
      background: transparent;
      border-bottom: 1px solid rgba(255, 255, 255, 0.02);
    }
    :host ::ng-deep .glass-table .p-datatable-tbody > tr > td {
      padding: 1rem;
    }
  `]
})
export class FechasLimiteComponent implements OnInit {
  private readonly fechasLimiteService = inject(FechasLimiteService);
  private readonly catalogService = inject(CatalogService);

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
        this.loadFechas();
      },
      error: (err) => {
        this.saving.set(false);
        alert('Error al guardar las configuraciones: ' + (err.error?.message || ''));
      }
    });
  }
}
