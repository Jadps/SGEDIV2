import { Component, inject, signal, computed, effect, input, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { MessageService } from 'primeng/api';
import { AlumnoService } from '../../../../core/services/alumno.service';
import { AuthService } from '../../../../core/services/auth.service';
import { ContratoService } from '../../../../core/services/contrato.service';
import { ContratoDto } from '../../../../core/models/contrato.dto';
import { ContratoResumenComponent } from '../contrato-profesor/contrato-resumen.component';
import { ContratoFormComponent } from '../contrato-profesor/contrato-form.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { FileUploaderComponent } from '../../../../shared/components/file-uploader/file-uploader.component';

@Component({
  selector: 'app-alumno-materia-docs-tab',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    SelectModule,
    TableModule,
    ButtonModule,
    TooltipModule,
    DialogModule,
    ContratoResumenComponent,
    ContratoFormComponent,
    StatusBadgeComponent,
    FileUploaderComponent
  ],
  templateUrl: './materia-docs.component.html'
})
export class AlumnoMateriaDocsTabComponent {
  private readonly alumnoService = inject(AlumnoService);
  private readonly authService = inject(AuthService);
  private readonly contratoService = inject(ContratoService);
  private readonly messageService = inject(MessageService);

  alumnoId = input.required<string>();
  materias = signal<any[]>([]);
  selectedMateriaId = signal<string | null>(null);
  contratoActual = signal<ContratoDto | null>(null);
  loadingAcuerdo = signal(false);
  loading = signal(false);
  editMode = signal(false);

  constructor() {
    effect(() => {
      const id = this.alumnoId();
      if (id) {
        untracked(() => this.loadCarga(id));
      }
    });
  }

  loadCarga(id: string) {
    this.alumnoService.getCargaAcademica(id).subscribe(carga => {
      this.materias.set(carga);
      if (carga.length > 0) {
        this.selectedMateriaId.set(carga[0].materiaId);
        this.loadContrato(carga[0].materiaId);
      }
    });
  }

  onMateriaChange() {
    if (this.selectedMateriaId()) {
      this.editMode.set(false);
      this.loadContrato(this.selectedMateriaId()!);
    }
  }

  loadContrato(materiaId: string) {
    this.loadingAcuerdo.set(true);
    this.contratoService.getContrato(this.alumnoId(), materiaId).subscribe({
      next: (contrato) => {
        this.contratoActual.set(contrato);
        this.loadingAcuerdo.set(false);
      },
      error: () => {
        this.contratoActual.set(null);
        this.loadingAcuerdo.set(false);
      }
    });
  }

  isProfessor = computed(() => this.authService.getUserRoles().some(r => r.toLowerCase() === 'profesor'));
  isAlumno = computed(() => this.authService.getUserRoles().some(r => r.toLowerCase() === 'alumno'));

  canStudentRespond = computed(() => {
    const isStudent = this.isAlumno();
    const isPendiente = this.contratoActual()?.estado?.toLowerCase() === 'pendiente';
    return isStudent && isPendiente;
  });

  filteredDocs = computed(() => {
    const contrato = this.contratoActual();
    if (!contrato) return [];
    
    // Logic to return annexes based on materiaId and alumnoId
    // For now returning mock data or fetching from service if implemented
    return [
      { label: 'Anexo III - ' + contrato.materiaNombre, estado: 0, estadoText: 'NO SUBIDO', estadoSeverity: 'secondary', fechaLimite: '2026-03-01', puedeSubir: true },
      { label: 'Anexo VII - ' + contrato.materiaNombre, estado: 0, estadoText: 'NO SUBIDO', estadoSeverity: 'secondary', fechaLimite: '2026-06-01', puedeSubir: true }
    ];
  });

  isDeadlineExpired(date: string) {
    return new Date(date) < new Date();
  }

  uploadVisible = signal(false);
  selectedDoc = signal<any>(null);
  selectedFile = signal<File | null>(null);
  isUploading = signal(false);

  viewDocument(id: string) { }

  openUpload(doc: any) {
    this.selectedDoc.set(doc);
    this.uploadVisible.set(true);
  }

  clearUpload() {
    this.selectedDoc.set(null);
    this.selectedFile.set(null);
  }

  saveUpload() {
    this.isUploading.set(true);
    setTimeout(() => {
      this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Documento subido correctamente.' });
      this.uploadVisible.set(false);
      this.isUploading.set(false);
    }, 1500);
  }
}
