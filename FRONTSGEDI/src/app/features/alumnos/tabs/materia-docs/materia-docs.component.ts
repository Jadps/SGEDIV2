import { Component, inject, input, signal, computed, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FormsModule } from '@angular/forms';
import { TooltipModule } from 'primeng/tooltip';
import { AlumnoService } from '../../../../core/services/alumno.service';
import { ExpedienteService } from '../../../../core/services/expediente.service';
import { DocumentUploadService } from '../../../../core/services/document-upload.service';
import { MessageService } from 'primeng/api';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { FileUploaderComponent } from '../../../../shared/components/file-uploader/file-uploader.component';
import { DocumentActionsService } from '../../../../core/services/document-actions.service';
import { DocumentoEstadoUtils } from '../../../../core/utils/documento-estado-utils';

@Component({
  selector: 'app-alumno-materia-docs-tab',
  standalone: true,
  imports: [
    CommonModule,
    SelectModule,
    TableModule,
    ButtonModule,
    DialogModule,
    FormsModule,
    TooltipModule,
    StatusBadgeComponent,
    FileUploaderComponent
  ],
  templateUrl: './materia-docs.component.html'
})
export class AlumnoMateriaDocsTabComponent {
  private readonly alumnoService = inject(AlumnoService);
  private readonly expedienteService = inject(ExpedienteService);
  private readonly uploadService = inject(DocumentUploadService);
  private readonly messageService = inject(MessageService);
  readonly docActions = inject(DocumentActionsService);

  alumnoId = input.required<string>();

  materias = signal<any[]>([]);
  selectedMateriaId = signal<string | null>(null);
  documents = signal<any[]>([]);
  loading = signal(false);

  uploadVisible = signal(false);
  selectedDoc = signal<any>(null);
  selectedFile = signal<File | null>(null);
  isUploading = signal(false);

  filteredDocs = computed(() => {
    const materiaId = this.selectedMateriaId();
    if (!materiaId) return [];
    return this.documents().filter(d => d.materiaId === materiaId);
  });

  constructor() {
    effect(() => {
      const id = this.alumnoId();
      if (id) {
        untracked(() => {
          this.loadMaterias();
          this.loadDocs();
        });
      }
    });
  }

  loadMaterias() {
    this.alumnoService.getCargaAcademica(this.alumnoId()).subscribe(data => {
      this.materias.set(data);
      if (data.length === 1) {
        this.selectedMateriaId.set(data[0].materiaId);
      }
    });
  }

  loadDocs() {
    this.loading.set(true);
    this.expedienteService.getExpediente(this.alumnoId()).subscribe({
      next: (data) => {
        this.documents.set(DocumentoEstadoUtils.mapExpediente(data));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onMateriaChange() { }

  openUpload(doc: any) {
    this.selectedDoc.set(doc);
    this.selectedFile.set(null);
    this.uploadVisible.set(true);
  }

  clearUpload() {
    this.selectedFile.set(null);
    this.selectedDoc.set(null);
  }

  saveUpload() {
    const doc = this.selectedDoc();
    const file = this.selectedFile();
    const materiaId = this.selectedMateriaId();
    if (!doc || !file || !materiaId) return;

    this.isUploading.set(true);
    this.uploadService.uploadProfesorAcuerdo(this.alumnoId(), doc.tipoId, file, materiaId).subscribe({
      next: () => {
        this.isUploading.set(false);
        this.uploadVisible.set(false);
        this.clearUpload();
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Documento subido correctamente' });
        this.loadDocs();
      },
      error: (err) => {
        this.isUploading.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.detail || 'Error al subir el documento' });
      }
    });
  }

  viewDocument(id: string) {
    this.docActions.viewDocument(id);
  }

  isDeadlineExpired(date: any) {
    return this.docActions.isDeadlineExpired(date);
  }
}

