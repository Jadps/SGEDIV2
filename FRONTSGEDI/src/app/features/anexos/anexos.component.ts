import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlantillaService } from '../../core/services/plantilla.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-anexos',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, DialogModule, SelectModule, ConfirmDialogModule, FormsModule],
  template: `
    <div class="p-8 h-full overflow-y-auto">
      <div class="max-w-6xl mx-auto">
        <div class="flex justify-between items-end mb-8">
          <div>
            <h1 class="text-3xl font-bold text-white mb-2">Plantillas de Anexos</h1>
            <p class="text-zinc-400">Gestiona los formatos oficiales que los alumnos descargarán.</p>
          </div>
          <p-button label="Nueva Plantilla" icon="pi pi-plus" (onClick)="showDialog()" />
        </div>

        <div class="bg-zinc-900/50 border border-white/5 rounded-2xl overflow-hidden backdrop-blur-xl">
          <p-table [value]="templates()" styleClass="p-datatable-sm glass-table" [loading]="loading()">
            <ng-template pTemplate="header">
              <tr>
                <th>Tipo</th>
                <th>Nombre del Archivo</th>
                <th class="w-24">Acciones</th>
              </tr>
            </ng-template>
            <ng-template pTemplate="body" let-temp>
              <tr class="transition-colors hover:bg-white/5">
                <td class="text-sm font-medium text-zinc-300">Anexo {{ temp.tipoDocumento }}</td>
                <td class="text-sm text-zinc-400">
                  <div class="flex items-center gap-2">
                    <i class="pi pi-file-word text-blue-400"></i>
                    {{ temp.nombre }}
                  </div>
                </td>
                <td class="flex gap-2">
                  <p-button icon="pi pi-download" [rounded]="true" [text]="true" severity="info" (onClick)="download(temp.id)" pTooltip="Descargar" />
                  <p-button icon="pi pi-pencil" [rounded]="true" [text]="true" severity="help" (onClick)="openEdit(temp)" pTooltip="Editar" />
                  <p-button icon="pi pi-trash" [rounded]="true" [text]="true" severity="danger" (onClick)="delete(temp.id)" pTooltip="Eliminar" />
                </td>
              </tr>
            </ng-template>
            <ng-template pTemplate="emptymessage">
              <tr>
                <td colspan="3" class="text-center py-12 text-zinc-500">No hay plantillas registradas.</td>
              </tr>
            </ng-template>
          </p-table>
        </div>
      </div>
    </div>

    <p-dialog [(visible)]="displayDialog" header="Subir Nueva Plantilla" [modal]="true" draggable="false" [style]="{width: '500px'}" styleClass="glass-dark border border-white/10 rounded-2xl overflow-hidden">
      <div class="flex flex-col gap-6 mt-4">
        <div class="flex flex-col gap-2">
          <label class="text-sm text-zinc-400">Tipo de Anexo</label>
          <p-select [options]="availableTipos()" [(ngModel)]="selectedTipo" optionLabel="label" optionValue="value" placeholder="Selecciona el anexo" styleClass="w-full bg-zinc-800/50 border-white/10" />
        </div>

        <div class="flex flex-col gap-2">
          <label class="text-sm text-zinc-400">Nombre Descriptivo</label>
          <input type="text" class="p-3 bg-zinc-800/50 border border-white/10 rounded-xl text-white outline-none focus:border-white/30" [(ngModel)]="nombrePlantilla" placeholder="Ej: Formato Carta Compromiso">
        </div>

        <div class="flex flex-col gap-2">
          <label class="text-sm text-zinc-400">Archivo Word (.doc, .docx)</label>
          <input type="file" #fileInput (change)="onFileSelected($event)" accept=".doc,.docx,.pdf" class="block w-full text-sm text-zinc-500
            file:mr-4 file:py-2 file:px-4
            file:rounded-full file:border-0
            file:text-sm file:font-semibold
            file:bg-indigo-500/10 file:text-indigo-400
            hover:file:bg-indigo-500/20
            cursor-pointer" />
        </div>
      </div>
      <ng-template pTemplate="footer">
        <p-button label="Cancelar" [text]="true" severity="secondary" (onClick)="displayDialog = false" />
        <p-button label="Subir Plantilla" (onClick)="upload()" [loading]="uploading()" [disabled]="!selectedTipo || !nombrePlantilla || !selectedFile" />
      </ng-template>
    </p-dialog>

    <p-dialog [(visible)]="editDialogVisible" header="Editar Nombre de Plantilla" [modal]="true" draggable="false" [style]="{width: '400px'}" styleClass="glass-dark border border-white/10 rounded-2xl overflow-hidden">
      <div class="flex flex-col gap-6 mt-4">
        <div class="flex flex-col gap-2">
          <label class="text-sm text-zinc-400">Nuevo Nombre</label>
          <input type="text" class="p-3 bg-zinc-800/50 border border-white/10 rounded-xl text-white outline-none focus:border-white/30" [(ngModel)]="editNombrePlantilla" placeholder="Ej: Formato Carta Compromiso">
        </div>
      </div>
      <ng-template pTemplate="footer">
        <p-button label="Cancelar" [text]="true" severity="secondary" (onClick)="editDialogVisible = false" />
        <p-button label="Guardar" (onClick)="saveEdit()" [loading]="uploading()" [disabled]="!editNombrePlantilla" />
      </ng-template>
    </p-dialog>
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
export class AnexosComponent implements OnInit {
  private readonly plantillaService = inject(PlantillaService);
  private readonly confirmationService = inject(ConfirmationService);

  templates = signal<any[]>([]);
  loading = signal(false);
  uploading = signal(false);

  displayDialog = false;
  editDialogVisible = false;

  tiposPlantilla = [
    { label: 'Anexo I', value: 1 },
    { label: 'Anexo II', value: 2 },
    { label: 'Anexo III', value: 3 },
    { label: 'Anexo IV', value: 4 },
    { label: 'Anexo V', value: 5 },
    { label: 'Anexo VI', value: 6 },
    { label: 'Anexo VII', value: 7 },
    { label: 'Anexo VIII', value: 8 }
  ];

  selectedTipo: number | null = null;
  nombrePlantilla = '';
  selectedFile: File | null = null;

  editingTemplateId: number | null = null;
  editNombrePlantilla = '';

  availableTipos = computed(() => {
    const existing = this.templates().map(t => t.tipoDocumento);
    return this.tiposPlantilla.filter(t => !existing.includes(t.value));
  });

  ngOnInit() {
    this.loadTemplates();
  }

  loadTemplates() {
    this.loading.set(true);
    this.plantillaService.getTemplates().subscribe({
      next: (data) => {
        this.templates.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  showDialog() {
    this.selectedTipo = null;
    this.nombrePlantilla = '';
    this.selectedFile = null;
    this.displayDialog = true;
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  upload() {
    if (!this.selectedTipo || !this.nombrePlantilla || !this.selectedFile) return;

    this.uploading.set(true);
    this.plantillaService.uploadTemplate(this.selectedTipo, this.nombrePlantilla, this.selectedFile).subscribe({
      next: () => {
        this.uploading.set(false);
        this.displayDialog = false;
        this.loadTemplates();
      },
      error: (err) => {
        this.uploading.set(false);
        alert(err.error?.message || 'Error al subir la plantilla');
      }
    });
  }

  download(id: number) {
    this.plantillaService.downloadTemplate(id);
  }

  openEdit(temp: any) {
    this.editingTemplateId = temp.id;
    this.editNombrePlantilla = temp.nombre;
    this.editDialogVisible = true;
  }

  saveEdit() {
    if (!this.editingTemplateId || !this.editNombrePlantilla) return;
    this.uploading.set(true);
    this.plantillaService.updateTemplate(this.editingTemplateId, this.editNombrePlantilla).subscribe({
      next: () => {
        this.uploading.set(false);
        this.editDialogVisible = false;
        this.loadTemplates();
      },
      error: () => this.uploading.set(false)
    });
  }

  delete(id: number) {
    this.confirmationService.confirm({
      message: '¿Estás seguro de eliminar esta plantilla? (Los anexos ya subidos por alumnos no se verán afectados)',
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      rejectLabel: 'Cancelar',
      rejectButtonProps: {
        label: 'Cancelar',
        severity: 'secondary',
        outlined: true,
      },
      acceptLabel: 'Eliminar',
      acceptButtonProps: {
        label: 'Eliminar',
        severity: 'danger',
      },
      accept: () => {
        this.plantillaService.deleteTemplate(id).subscribe(() => this.loadTemplates());
      }
    });
  }
}
