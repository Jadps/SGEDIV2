import { Component, inject, OnInit, signal, computed, model } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlumnoService } from '../../core/services/alumno.service';
import { AnexoMetaService } from '../../core/services/anexo-meta.service';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { FileUploaderComponent } from '../../shared/components/file-uploader/file-uploader.component';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-mis-documentos',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, StatusBadgeComponent, DialogModule, FileUploaderComponent, TooltipModule],
  templateUrl: './mis-documentos.component.html',
  styleUrl: './mis-documentos.component.css'
})
export class MisDocumentosComponent implements OnInit {
  private readonly alumnoService = inject(AlumnoService);
  private readonly messageService = inject(MessageService);
  readonly anexoMeta = inject(AnexoMetaService);

  profile = signal<any>(null);
  documents = signal<any[]>([]);
  templates = signal<any[]>([]);
  deadlines = signal<any[]>([]);
  loading = signal(false);

  mergedDocuments = computed(() => {
    const realDocs = this.documents();
    const scheduledDeadlines = this.deadlines();

    return this.anexoMeta.expectedDocuments.map(expected => {
      const real = realDocs.find(d =>
        (expected.esAcuerdo && d.esAcuerdo && d.tipoId === expected.tipoId) ||
        (!expected.esAcuerdo && !d.esAcuerdo && d.tipoId === expected.tipoId)
      );
      const scheduled = scheduledDeadlines.find(sd => sd.tipoAcuerdo === expected.tipoId);

      return real ? { ...real, label: expected.label } : {
        label: expected.label,
        tipo: expected.label,
        tipoId: expected.tipoId,
        esAcuerdo: expected.esAcuerdo,
        estado: -1,
        version: 0,
        semestre: 'Pendiente',
        fechaLimite: scheduled?.fechaLimite
      };
    });
  });

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

    this.alumnoService.getDocuments(alumnoId).subscribe({
      next: (data) => this.documents.set(data),
      error: () => this.loading.set(false)
    });

    this.alumnoService.getMyDeadlines().subscribe({
      next: (data) => {
        this.deadlines.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadTemplates() {
    this.alumnoService.getTemplates().subscribe(data => this.templates.set(data));
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

    const upload$ = doc.esAcuerdo
      ? this.alumnoService.uploadStudentAcuerdo(doc.tipoId, file)
      : this.alumnoService.uploadAdministrativePersonalDoc(alumnoId, doc.tipoId, file);

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
    this.alumnoService.downloadDocument(id).subscribe({
      next: (blob) => window.open(window.URL.createObjectURL(blob), '_blank'),
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al cargar el documento' })
    });
  }

  downloadTemplate(id: number) {
    this.alumnoService.downloadTemplate(id);
  }

  isDeadlineExpired(date: string | Date | null | undefined): boolean {
    if (!date) return false;
    const d = typeof date === 'string' ? new Date(date) : date;
    return d < new Date();
  }
}