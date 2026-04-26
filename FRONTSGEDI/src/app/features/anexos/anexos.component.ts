import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlantillaService } from '../../core/services/plantilla.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService, MessageService } from 'primeng/api';
import { FormsModule } from '@angular/forms';
import { TooltipModule } from 'primeng/tooltip';
import { FileUploaderComponent } from '../../shared/components/file-uploader/file-uploader.component';

@Component({
  selector: 'app-anexos',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    DialogModule,
    SelectModule,
    FormsModule,
    TooltipModule,
    FileUploaderComponent
  ],
  templateUrl: './anexos.component.html',
  styleUrl: './anexos.component.css'
})
export class AnexosComponent implements OnInit {
  private readonly plantillaService = inject(PlantillaService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

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
  selectedFile: File | null = null;

  editingTemplateId: number | null = null;
  editingTemplateName = '';
  editSelectedFile: File | null = null;

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
    this.selectedFile = null;
    this.displayDialog = true;
  }

  upload() {
    if (!this.selectedTipo || !this.selectedFile) return;

    this.uploading.set(true);
    this.plantillaService.uploadTemplate(this.selectedTipo, this.selectedFile).subscribe({
      next: () => {
        this.uploading.set(false);
        this.displayDialog = false;
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Plantilla subida correctamente' });
        this.loadTemplates();
      },
      error: (err) => {
        this.uploading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message || 'Error al subir la plantilla'
        });
      }
    });
  }

  download(id: number) {
    this.plantillaService.downloadTemplate(id);
  }

  openEdit(temp: any) {
    this.editingTemplateId = temp.id;
    this.editingTemplateName = this.formatTipo(temp.tipo);
    this.editSelectedFile = null;
    this.editDialogVisible = true;
  }

  saveEdit() {
    if (!this.editingTemplateId || !this.editSelectedFile) return;
    this.uploading.set(true);
    this.plantillaService.updateTemplate(this.editingTemplateId, this.editSelectedFile).subscribe({
      next: () => {
        this.uploading.set(false);
        this.editDialogVisible = false;
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Archivo actualizado correctamente' });
        this.loadTemplates();
      },
      error: (err) => {
        this.uploading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.message || 'Error al actualizar la plantilla'
        });
      }
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

  formatTipo(tipo: string | null | undefined): string {
    if (!tipo) return '';
    return tipo.replace(/Anexo([I|V|X]+)/, 'Anexo $1');
  }
}
