import { Component, signal, inject, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { AlumnoService } from '../../core/services/alumno.service';
import { CatalogService, ProfesorCatalogDto } from '../../core/services/catalog.service';
import { MessageService } from 'primeng/api';
import { ProfessorDetailComponent } from './detail/detail.component';
import { rxResource } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { CargaAcademicaService } from '../../core/services/carga-academica.service';

@Component({
  selector: 'app-professor-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    ButtonModule, SelectModule, TableModule,
    ProfessorDetailComponent
  ],
  templateUrl: './professor-list.component.html',
})
export class ProfessorListComponent implements OnInit {
  private readonly studentService = inject(AlumnoService);
  private readonly cargaService = inject(CargaAcademicaService);
  private readonly catalogService = inject(CatalogService);
  private readonly toast = inject(MessageService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  isLoading = signal(false);
  isSaving = signal(false);
  isEditing = signal(false);
  careerId = signal<number | null>(null);

  academicLoad = signal<any[]>([]);
  selectedProfessor = signal<any | null>(null);
  showDetail = signal(false);

  subjectsResource = rxResource<any[], { id: number | null, editing: boolean }>({
    params: () => ({ id: this.careerId(), editing: this.isEditing() }),
    stream: ({ params: { id, editing } }) => (editing && id) ? this.catalogService.getMaterias(id) : of([])
  });

  professorsResource = rxResource<ProfesorCatalogDto[], boolean>({
    params: () => this.isEditing(),
    stream: ({ params: editing }) => editing ? this.catalogService.getProfesores() : of([])
  });

  loadForm = this.fb.group({
    items: this.fb.array([])
  });

  get items() {
    return this.loadForm.get('items') as FormArray;
  }

  get totalCredits(): number {
    const allSubjects = this.subjectsResource.value() || [];
    const selectedIds = this.items.controls.map(c => c.get('materiaId')?.value).filter(id => id);
    return selectedIds.reduce((sum, id) => {
      const subject = allSubjects.find(s => s.id === id);
      return sum + (subject ? subject.creditos : 0);
    }, 0);
  }

  get canAddRow(): boolean {
    return this.items.length < 8 && this.totalCredits < 36;
  }

  getAvailableSubjects(index: number): any[] {
    const allSubjects = this.subjectsResource.value() || [];
    const selectedSubjectIds = this.items.controls
      .map((c, i) => (i !== index ? c.get('materiaId')?.value : null))
      .filter(id => id);
    return allSubjects.filter(s => !selectedSubjectIds.includes(s.id));
  }

  ngOnInit() {
    this.studentService.getMyProfile().subscribe(profile => {
      this.careerId.set(profile.carreraId);
      this.fetchLoad();
    });
  }

  fetchLoad() {
    this.isLoading.set(true);
    this.cargaService.getCargaAcademica('me').subscribe({
      next: (data) => {
        this.academicLoad.set(data);
        this.items.clear();
        if (data.length === 0) {
          this.isEditing.set(true);
          this.addRow();
        } else {
          this.isEditing.set(false);
          data.forEach(item => {
            this.addRow(item.materiaId, item.profesorId);
          });
        }
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  addRow(subjectId: string = '', professorId: string = '') {
    const row = this.fb.group({
      materiaId: [subjectId, Validators.required],
      profesorId: [professorId, Validators.required]
    });
    this.items.push(row);
  }

  removeRow(index: number) {
    this.items.removeAt(index);
  }

  toggleEdit() {
    if (this.isEditing()) {
      this.fetchLoad();
    } else {
      this.isEditing.set(true);
    }
  }

  save() {
    if (this.loadForm.invalid) return;

    this.isSaving.set(true);
    const payload = this.items.value;
    this.cargaService.setCargaAcademica(payload).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.toast.add({ severity: 'success', summary: 'Éxito', detail: 'Carga académica guardada. Tus Anexos III y VII ya están disponibles.' });
        this.router.navigate(['/mis-documentos']);
      },
      error: () => {
        this.isSaving.set(false);
        this.toast.add({ severity: 'error', summary: 'Error', detail: 'No se pudo guardar la carga académica.' });
      }
    });
  }

  viewDetail(item: any) {
    this.selectedProfessor.set(item);
    this.showDetail.set(true);
  }
}
