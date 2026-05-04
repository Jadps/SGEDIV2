import { Component, inject, OnInit, signal, computed, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlantillaService } from '../../core/services/plantilla.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
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
  templateUrl: './anexos.component.html'
})
export class AnexosComponent implements OnInit {
  private readonly plantillaService = inject(PlantillaService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly messageService = inject(MessageService);

  templates = signal<any[]>([]);
  loading = signal(false);
  uploading = signal(false);

  displayDialog = model(false);
  editDialogVisible = model(false);

  selectedTipo = model<number | null>(null);
  selectedFile = signal<File | null>(null);

  editingTemplateId = signal<number | null>(null);
  editingTemplateName = signal('');
  editSelectedFile = signal<File | null>(null);
  allTipos = signal<any[]>([]);

  availableTipos = computed(() => {
    const existing = this.templates().map(t => t.tipoDocumento);
    return this.allTipos().filter(t => !existing.includes(t.value));
  });


  ngOnInit() {
    this.loadTemplates();
    this.loadTypes();
  }

  loadTypes() {
    this.plantillaService.getTemplateTypes().subscribe(data => this.allTipos.set(data));
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
    this.selectedTipo.set(null);
    this.selectedFile.set(null);
    this.displayDialog.set(true);
  }

  upload() {
    const tipo = this.selectedTipo();
    const file = this.selectedFile();
    if (!tipo || !file) return;

    this.uploading.set(true);
    this.plantillaService.uploadTemplate(tipo, file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.displayDialog.set(false);
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Plantilla subida correctamente' });
        this.loadTemplates();
      },
      error: (err) => {
        this.uploading.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Error al subir la plantilla' });
      }
    });
  }

  download(id: number) {
    this.plantillaService.downloadTemplate(id);
  }

  openEdit(temp: any) {
    this.editingTemplateId.set(temp.id);
    this.editingTemplateName.set(temp.label);
    this.editSelectedFile.set(null);
    this.editDialogVisible.set(true);
  }

  saveEdit() {
    const id = this.editingTemplateId();
    const file = this.editSelectedFile();
    if (!id || !file) return;

    this.uploading.set(true);
    this.plantillaService.updateTemplate(id, file).subscribe({
      next: () => {
        this.uploading.set(false);
        this.editDialogVisible.set(false);
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Archivo actualizado correctamente' });
        this.loadTemplates();
      },
      error: (err) => {
        this.uploading.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Error al actualizar la plantilla' });
      }
    });
  }

  delete(id: number) {
    this.confirmationService.confirm({
      message: '¿Estás seguro de eliminar esta plantilla? (Los anexos ya subidos por alumnos no se verán afectados)',
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      rejectLabel: 'Cancelar',
      rejectButtonProps: { label: 'Cancelar', severity: 'secondary', outlined: true },
      acceptLabel: 'Eliminar',
      acceptButtonProps: { label: 'Eliminar', severity: 'danger' },
      accept: () => {
        this.plantillaService.deleteTemplate(id).subscribe(() => this.loadTemplates());
      }
    });
  }
}