import { Component, inject, OnInit, signal, computed, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpContext } from '@angular/common/http';
import { AlumnoService } from '../../core/services/alumno.service';
import { ExpedienteService } from '../../core/services/expediente.service';
import { DocumentUploadService } from '../../core/services/document-upload.service';

import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { FileUploaderComponent } from '../../shared/components/file-uploader/file-uploader.component';
import { TooltipModule } from 'primeng/tooltip';
import { DocumentActionsService } from '../../core/services/document-actions.service';
import { SKIP_ERROR_NOTIFICATION } from '../../core/constants/http-context';
import { DocumentoEstadoUtils } from '../../core/utils/documento-estado-utils';


@Component({
  selector: 'app-mis-documentos',
  standalone: true,
  imports: [CommonModule, RouterModule, TableModule, ButtonModule, StatusBadgeComponent, DialogModule, FileUploaderComponent, TooltipModule],
  templateUrl: './mis-documentos.component.html',
  styleUrl: './mis-documentos.component.css'
})
export class MisDocumentosComponent implements OnInit {
  private readonly alumnoService = inject(AlumnoService);
  private readonly expedienteService = inject(ExpedienteService);
  private readonly uploadService = inject(DocumentUploadService);
  private readonly messageService = inject(MessageService);
  readonly docActions = inject(DocumentActionsService);

  profile = signal<any>(null);
  expediente = signal<any[]>([]);
  templates = signal<any[]>([]);
  loading = signal(false);

  uploadDialogVisible = model(false);
  selectedDocForUpload = signal<any>(null);
  isUploading = signal(false);
  selectedFile = signal<File | null>(null);

  ngOnInit() {
    this.loadProfile();
    this.loadTemplates();
  }

  loadProfile() {
    this.loading.set(true);
    this.alumnoService.getMyProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.loadDocs();
      },
      error: () => this.loading.set(false)
    });
  }

  loadDocs() {
    const alumnoId = this.profile()?.id;
    if (!alumnoId) return;

    this.expedienteService.getExpediente(alumnoId).subscribe({
      next: (data) => {
        this.expediente.set(DocumentoEstadoUtils.mapExpediente(data));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadTemplates() {
    this.uploadService.getTemplates().subscribe(data => this.templates.set(data));
  }

  openUpload(doc: any) {
    this.selectedDocForUpload.set(doc);
    this.selectedFile.set(null);
    this.uploadDialogVisible.set(true);
  }

  saveUpload() {
    const doc = this.selectedDocForUpload();
    const file = this.selectedFile();
    const alumnoId = this.profile()?.id;
    if (!doc || !file || !alumnoId) return;

    this.isUploading.set(true);
    const context = new HttpContext().set(SKIP_ERROR_NOTIFICATION, true);

    const upload$ = doc.esAcuerdo
      ? this.uploadService.uploadStudentAcuerdo(doc.tipoId, file, context)
      : this.uploadService.uploadAdministrativePersonalDoc(alumnoId, doc.tipoId, file, context);


    upload$.subscribe({
      next: () => {
        this.isUploading.set(false);
        this.uploadDialogVisible.set(false);
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

  downloadTemplate(id: number) {
    this.docActions.downloadTemplate(id);
  }

  isDeadlineExpired(date: string | Date | null | undefined): boolean {
    return this.docActions.isDeadlineExpired(date);
  }
}