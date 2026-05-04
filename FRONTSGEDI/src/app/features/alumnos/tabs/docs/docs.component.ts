import { Component, inject, input, signal, computed, model, effect, untracked } from '@angular/core';

import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ExpedienteService } from '../../../../core/services/expediente.service';
import { DocumentUploadService } from '../../../../core/services/document-upload.service';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { FileUploaderComponent } from '../../../../shared/components/file-uploader/file-uploader.component';
import { DocumentActionsService } from '../../../../core/services/document-actions.service';
import { DocumentoEstadoUtils } from '../../../../core/utils/documento-estado-utils';

@Component({
  selector: 'app-alumno-docs-tab',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, StatusBadgeComponent, DialogModule, TextareaModule, FormsModule, SelectModule, FileUploaderComponent],
  templateUrl: './docs.component.html',
  styleUrl: './docs.component.css'
})
export class AlumnoDocsTabComponent {

  private readonly expedienteService = inject(ExpedienteService);
  private readonly uploadService = inject(DocumentUploadService);
  private readonly messageService = inject(MessageService);
  readonly docActions = inject(DocumentActionsService);

  alumnoId = input.required<string>();
  isMyCareer = input<boolean>(false);
  isAdmin = input<boolean>(false);

  documents = signal<any[]>([]);
  templates = signal<any[]>([]);
  loading = signal(false);

  prorrogaDialogVisible = signal(false);
  selectedDocForProrroga = signal<any>(null);
  selectedFechaLimite = signal<string>('');

  rejectDialogVisible = signal(false);
  motivoRechazo = signal('');
  docToReview = signal<any>(null);

  canReview = computed(() => this.isAdmin() || this.isMyCareer());

  adminUploadVisible = signal(false);
  selectedDocForAdminUpload = signal<any>(null);
  adminSelectedFile = signal<File | null>(null);
  isUploadingAdmin = signal(false);

  availableSemestres = signal<string[]>([]);
  selectedSemestre = model<string | null>(null);

  constructor() {
    effect(() => {
      const id = this.alumnoId();
      if (id) {
        untracked(() => {
          this.selectedSemestre.set(null);
          this.loadSemestres();
          this.loadDocs();
          this.loadTemplates();
        });
      }
    });
  }


  loadSemestres() {
    if (!this.canReview()) return;
    this.expedienteService.getSemestres(this.alumnoId()).subscribe(data => {
      this.availableSemestres.set(data);
    });
  }

  onSemestreChange() {
    this.loadDocs();
  }

  loadDocs() {
    this.loading.set(true);
    const semestre = this.selectedSemestre();
    this.expedienteService.getExpediente(this.alumnoId(), semestre || undefined).subscribe({
      next: (data) => {
        this.documents.set(DocumentoEstadoUtils.mapExpediente(data));
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  loadTemplates() {
    this.uploadService.getTemplates().subscribe(data => this.templates.set(data));
  }

  openProrroga(doc: any) {
    this.selectedDocForProrroga.set(doc);
    this.selectedFechaLimite.set('');
    this.prorrogaDialogVisible.set(true);
  }

  onFechaLimiteChange(event: Event) {
    this.selectedFechaLimite.set((event.target as HTMLInputElement).value);
  }

  saveProrroga() {
    const doc = this.selectedDocForProrroga();
    if (!doc || !this.selectedFechaLimite()) return;

    this.expedienteService.extendDeadline({
      acuerdoId: doc.documentoId,
      alumnoId: this.alumnoId(),
      tipoAcuerdo: doc.tipoId,
      semestre: doc.semestre || this.selectedSemestre() || undefined,
      nuevaFechaLimite: new Date(this.selectedFechaLimite())
    }).subscribe({
      next: () => {
        this.prorrogaDialogVisible.set(false);
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Prórroga otorgada correctamente' });
        this.loadDocs();
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo otorgar la prórroga' })
    });
  }

  isDeadlineExpired(date: string | Date | null | undefined): boolean {
    return this.docActions.isDeadlineExpired(date);
  }

  review(doc: any, aprobado: boolean) {
    if (aprobado) {
      this.expedienteService.reviewDocument(this.alumnoId(), doc.archivo.id, aprobado, undefined).subscribe(() => this.loadDocs());
    } else {
      this.docToReview.set(doc);
      this.motivoRechazo.set('');
      this.rejectDialogVisible.set(true);
    }
  }

  confirmReject() {
    const doc = this.docToReview();
    const motivo = this.motivoRechazo();
    if (!doc || !motivo) return;

    this.expedienteService.reviewDocument(this.alumnoId(), doc.archivo.id, false, motivo).subscribe(() => {
      this.rejectDialogVisible.set(false);
      this.loadDocs();
    });
  }

  viewDocument(id: string) {
    this.docActions.viewDocument(id);
  }

  download(id: number) {
    this.docActions.downloadTemplate(id);
  }

  openAdminUpload(doc: any) {
    this.selectedDocForAdminUpload.set(doc);
    this.adminSelectedFile.set(null);
    this.adminUploadVisible.set(true);
  }

  saveAdminUpload() {
    const doc = this.selectedDocForAdminUpload();
    const file = this.adminSelectedFile();
    if (!doc || !file) return;

    this.isUploadingAdmin.set(true);
    const upload$ = doc.esAcuerdo
      ? this.uploadService.uploadAdministrativeAcuerdo(doc.archivo?.id, file)
      : this.uploadService.uploadAdministrativePersonalDoc(this.alumnoId(), doc.tipoId, file);

    upload$.subscribe({
      next: () => {
        this.isUploadingAdmin.set(false);
        this.adminUploadVisible.set(false);
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Documento subido y validado correctamente' });
        this.loadDocs();
      },
      error: (err) => {
        this.isUploadingAdmin.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Error al subir el documento' });
      }
    });
  }
}