import { Component, inject, input, signal, computed, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageService } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { EvaluacionService, EvaluacionDto } from '../../../../core/services/evaluacion.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-alumno-evaluaciones-tab',
  standalone: true,
  imports: [
    CommonModule, TableModule, ButtonModule, DialogModule, SelectModule,
    FormsModule, ReactiveFormsModule, TextareaModule, InputNumberModule
  ],
  templateUrl: './evaluaciones.component.html'
})
export class AlumnoEvaluacionesTabComponent {
  private readonly evaluacionService = inject(EvaluacionService);
  private readonly authService = inject(AuthService);
  private readonly messageService = inject(MessageService);
  private readonly fb = inject(FormBuilder);

  alumnoId = input.required<string>();
  isMyCareer = input<boolean>(false);
  isAdmin = input<boolean>(false);

  evaluaciones = signal<EvaluacionDto[]>([]);
  loading = signal(false);

  showDialog = signal(false);
  isSubmitting = signal(false);

  selectedSemestre = signal<string | null>(null);

  availableSemestres = computed(() => {
    const semestres = this.evaluaciones().map(e => e.semestre);
    return [...new Set(semestres)].sort().reverse();
  });

  filteredEvaluaciones = computed(() => {
    const selected = this.selectedSemestre();
    const all = this.evaluaciones();
    if (!selected) return all;
    return all.filter(e => e.semestre === selected);
  });

  evalForm = this.fb.group({
    calificacion: [null as number | null, [Validators.required, Validators.min(70), Validators.max(100)]],
    observaciones: ['', [Validators.required, Validators.maxLength(1000)]]
  });

  canEvaluate = computed(() => {
    const roles = this.authService.getUserRoles();
    const isAdvisor = roles.includes('Asesor Interno') || roles.includes('Asesor Externo');
    if (!isAdvisor) return false;

    const currentMonth = new Date().getMonth() + 1;
    return currentMonth === 6 || currentMonth === 11;
  });

  constructor() {
    effect(() => {
      const id = this.alumnoId();
      if (id) {
        untracked(() => this.loadEvaluaciones());
      }
    });
  }

  loadEvaluaciones() {
    this.loading.set(true);
    this.evaluacionService.getEvaluaciones(this.alumnoId()).subscribe({
      next: (data) => {
        this.evaluaciones.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openNew() {
    this.evalForm.reset();
    this.showDialog.set(true);
  }

  save() {
    if (this.evalForm.invalid) return;

    this.isSubmitting.set(true);
    const val = this.evalForm.value;

    this.evaluacionService.submitEvaluacion(this.alumnoId(), {
      calificacion: val.calificacion!,
      observaciones: val.observaciones!
    }).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.showDialog.set(false);
        this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Evaluación registrada correctamente' });
        this.loadEvaluaciones();
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.detail || 'Error al registrar evaluación' });
      }
    });
  }
}
