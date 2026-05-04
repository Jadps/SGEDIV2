import { Component, signal, inject, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormArray, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { AlumnoService } from '../../core/services/alumno.service';
import { CatalogService } from '../../core/services/catalog.service';
import { UserManagementService } from '../../core/services/user-management.service';
import { MessageService } from 'primeng/api';
import { MateriaDto } from '../../core/models/materia.dto';
import { InternalUserDto } from '../../core/models/internal-user.dto';
import { ProfessorDetailComponent } from './detail/detail.component';
import { rxResource } from '@angular/core/rxjs-interop';
import { map, of } from 'rxjs';

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
  private readonly catalogService = inject(CatalogService);
  private readonly userService = inject(UserManagementService);
  private readonly toast = inject(MessageService);
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

  professorsResource = rxResource<any[], boolean>({
    params: () => this.isEditing(),
    stream: ({ params: editing }) => editing ? this.userService.getInternalUsers().pipe(
      map(users => users.filter(u => u.roles.includes('Profesor')))
    ) : of([])
  });

  loadForm = this.fb.group({
    items: this.fb.array([])
  });

  get items() {
    return this.loadForm.get('items') as FormArray;
  }

  ngOnInit() {
    this.studentService.getMyProfile().subscribe(profile => {
      this.careerId.set(profile.carreraId);
      this.fetchLoad();
    });
  }

  fetchLoad() {
    this.isLoading.set(true);
    this.studentService.getCargaAcademica('me').subscribe({
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
    this.studentService.setCargaAcademica(payload).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.fetchLoad();
        this.toast.add({ severity: 'success', summary: 'Success', detail: 'Academic load saved successfully' });
      },
      error: () => {
        this.isSaving.set(false);
        this.toast.add({ severity: 'error', summary: 'Error', detail: 'Could not save academic load' });
      }
    });
  }

  viewDetail(item: any) {
    this.selectedProfessor.set(item);
    this.showDetail.set(true);
  }
}
