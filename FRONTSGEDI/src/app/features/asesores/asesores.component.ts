import { Component, inject, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { AlumnoService } from '../../core/services/alumno.service';
import { EvaluacionService, EvaluacionDto } from '../../core/services/evaluacion.service';
import { AlumnoDetailDto } from '../../core/models/alumno-detail.dto';

@Component({
  selector: 'app-mis-asesores',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './asesores.component.html'
})
export class MisAsesoresComponent implements OnInit {
  private readonly studentService = inject(AlumnoService);
  private readonly evaluacionService = inject(EvaluacionService);

  student = signal<AlumnoDetailDto | null>(null);
  isLoading = signal(false);

  evaluacionesResource = rxResource<EvaluacionDto[], string | null | undefined>({
    params: () => this.student()?.id,
    stream: ({ params: id }) => id ? this.evaluacionService.getEvaluaciones(id) : of([])
  });

  lastInternalEval = computed(() => {
    const evals = this.evaluacionesResource.value() || [];
    return evals.find(e => e.evaluadorRol === 'Asesor Interno');
  });

  lastExternalEval = computed(() => {
    const evals = this.evaluacionesResource.value() || [];
    return evals.find(e => e.evaluadorRol === 'Asesor Externo');
  });

  ngOnInit() {
    this.isLoading.set(true);
    this.studentService.getMyProfile().subscribe({
      next: (profile) => {
        this.studentService.getAlumno(profile.id).subscribe({
          next: (detail) => {
            this.student.set(detail);
            this.isLoading.set(false);
          },
          error: () => this.isLoading.set(false)
        });
      },
      error: () => this.isLoading.set(false)
    });
  }
}
