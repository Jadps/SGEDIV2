import { Component, inject, input, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { AlumnoService } from '../../../../core/services/alumno.service';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { FormsModule } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { FileUploaderComponent } from '../../../../shared/components/file-uploader/file-uploader.component';

@Component({
  selector: 'app-alumno-docs-tab',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule, StatusBadgeComponent, DialogModule, TextareaModule, FormsModule, SelectModule, FileUploaderComponent],
  templateUrl: './docs.component.html',
  styleUrl: './docs.component.css'
})
export class AlumnoDocsTabComponent implements OnInit {
  private readonly alumnoService = inject(AlumnoService);
  private readonly messageService = inject(MessageService);

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
  selectedSemestre = signal<string | null>(null);

  ngOnInit() {
    this.loadSemestres();
    this.loadDocs();
    this.loadTemplates();
  }

  loadSemestres() {
    if (!this.canReview()) return;
    this.alumnoService.getSemestres(this.alumnoId()).subscribe(data => {
      this.availableSemestres.set(data);
    });
  }

  onSemestreChange() {
    this.loadDocs();
  }

  loadDocs() {
    this.loading.set(true);
    const semestre = this.selectedSemestre();
    this.alumnoService.getDocuments(this.alumnoId(), semestre || undefined).subscribe({
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

  openProrroga(doc: any) {
    this.selectedDocForProrroga.set(doc);
    this.selectedFechaLimite.set('');
    this.prorrogaDialogVisible.set(true);
  }

  onFechaLimiteChange(event: Event) {
    const val = (event.target as HTMLInputElement).value;
    this.selectedFechaLimite.set(val);
  }

  saveProrroga() {
    const doc = this.selectedDocForProrroga();
    if (!doc || !this.selectedFechaLimite()) return;

    const newDate = new Date(this.selectedFechaLimite());
    this.alumnoService.extendDeadline(doc.id, newDate).subscribe({
      next: () => {
        this.prorrogaDialogVisible.set(false);
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Prórroga otorgada correctamente' });
        this.loadDocs();
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'No se pudo otorgar la prórroga' })
    });
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
    if (aprobado) {
      this.alumnoService.reviewDocument(this.alumnoId(), doc.id, aprobado, undefined).subscribe(() => {
        this.loadDocs();
      });
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

    this.alumnoService.reviewDocument(this.alumnoId(), doc.id, false, motivo).subscribe(() => {
      this.rejectDialogVisible.set(false);
      this.loadDocs();
    });
  }

  viewDocument(id: string) {
    this.alumnoService.downloadDocument(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al cargar el documento' })
    });
  }

  download(id: number) {
    this.alumnoService.downloadTemplate(id);
  }

  formatTipo(tipo: string | null | undefined): string {
    if (!tipo) return '';
    return tipo.replace(/Anexo([I|V|X]+)/, 'Anexo $1');
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
      ? this.alumnoService.uploadAdministrativeAcuerdo(doc.id, file)
      : this.alumnoService.uploadAdministrativePersonalDoc(this.alumnoId(), doc.tipoId, file);

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
