import { Component, inject, input, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { AlumnoService } from '../../../../core/services/alumno.service';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-alumno-docs-tab',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, StatusBadgeComponent],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <h3 class="text-lg font-medium text-white">Documentos del Alumno</h3>
        <div class="flex gap-2">
            <!-- Template download options could go here -->
        </div>
      </div>

      <p-table [value]="documents()" styleClass="p-datatable-sm glass-table" [loading]="loading()">
        <ng-template pTemplate="header">
          <tr>
            <th>Documento</th>
            <th>Semestre</th>
            <th>Versión</th>
            <th>Estado</th>
            <th>Fecha</th>
            <th class="w-24"></th>
          </tr>
        </ng-template>
        <ng-template pTemplate="body" let-doc>
          <tr class="transition-colors hover:bg-white/5">
            <td class="text-sm font-medium text-zinc-300">{{ doc.tipo }}</td>
            <td class="text-xs text-zinc-400">{{ doc.semestre }}</td>
            <td class="text-xs text-zinc-400">v{{ doc.version }}</td>
            <td>
              <app-status-badge [severity]="getSeverity(doc.estado)" [text]="getEstadoText(doc.estado)" />
            </td>
            <td class="text-xs text-zinc-500">{{ doc.fechaSubida | date:'shortDate' }}</td>
            <td class="flex gap-2">
                <p-button icon="pi pi-eye" [rounded]="true" [text]="true" severity="info" (onClick)="viewDocument(doc.id)" pTooltip="Ver Documento" />
                @if (canReview() && doc.estado === 0) {
                    <p-button icon="pi pi-check" [rounded]="true" [text]="true" severity="success" (onClick)="review(doc, true)" pTooltip="Aprobar" />
                    <p-button icon="pi pi-times" [rounded]="true" [text]="true" severity="danger" (onClick)="review(doc, false)" pTooltip="Rechazar" />
                }
            </td>
          </tr>
        </ng-template>
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="6" class="text-center py-8 text-zinc-500">No hay documentos registrados.</td>
          </tr>
        </ng-template>
      </p-table>

      <div class="mt-8">
        <h3 class="text-sm font-medium text-zinc-400 mb-4 uppercase tracking-wider">Plantillas Disponibles</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            @for (temp of templates(); track temp.id) {
                <div class="flex items-center justify-between p-4 rounded-xl border border-white/5 bg-white/5 hover:bg-white/10 transition-colors">
                    <div class="flex items-center gap-3">
                        <i class="pi pi-file-pdf text-red-400 text-xl"></i>
                        <div>
                            <span class="block text-sm text-white font-medium">{{ temp.nombre }}</span>
                            <span class="text-[10px] text-zinc-500 uppercase">{{ temp.tipo }}</span>
                        </div>
                    </div>
                    <p-button icon="pi pi-download" [rounded]="true" [text]="true" (onClick)="download(temp.id)" />
                </div>
            }
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host ::ng-deep .glass-table .p-datatable-thead > tr > th {
      background: transparent;
      color: var(--zinc-400);
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);
      font-size: 0.75rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    :host ::ng-deep .glass-table .p-datatable-tbody > tr {
      background: transparent;
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
    }
  `]
})
export class AlumnoDocsTabComponent implements OnInit {
  private readonly alumnoService = inject(AlumnoService);

  alumnoId = input.required<string>();
  isMyCareer = input<boolean>(false);
  isAdmin = input<boolean>(false);

  documents = signal<any[]>([]);
  templates = signal<any[]>([]);
  loading = signal(false);

  canReview = computed(() => this.isAdmin() || this.isMyCareer());

  ngOnInit() {
    this.loadDocs();
    this.loadTemplates();
  }

  loadDocs() {
    this.loading.set(true);
    this.alumnoService.getDocuments(this.alumnoId()).subscribe({
      next: (data) => {
        this.documents.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadTemplates() {
    this.alumnoService.getTemplates().subscribe(data => this.templates.set(data));
  }

  getSeverity(estado: number): string {
    switch (estado) {
      case 1: return 'success';
      case 2: return 'danger';
      default: return 'warning';
    }
  }

  getEstadoText(estado: number): string {
    switch (estado) {
      case 1: return 'Aprobado';
      case 2: return 'Rechazado';
      default: return 'Pendiente';
    }
  }

  review(doc: any, aprobado: boolean) {
    const motivo = aprobado ? undefined : prompt('Motivo del rechazo:');
    if (!aprobado && !motivo) return;

    this.alumnoService.reviewDocument(this.alumnoId(), doc.id, aprobado, motivo!).subscribe(() => {
      this.loadDocs();
    });
  }

  viewDocument(id: string) {
    this.alumnoService.downloadDocument(id).subscribe({
        next: (blob) => {
            const url = window.URL.createObjectURL(blob);
            window.open(url, '_blank');
        },
        error: () => alert('Error al cargar el documento')
    });
  }

  download(id: number) {
    this.alumnoService.downloadTemplate(id);
  }
}
