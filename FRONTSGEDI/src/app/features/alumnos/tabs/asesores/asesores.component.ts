import { Component, inject, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlumnoDetailDto } from '../../../../core/models/alumno-detail.dto';
import { EvaluacionService, EvaluacionDto } from '../../../../core/services/evaluacion.service';
import { rxResource } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';

@Component({
  selector: 'app-alumno-asesores-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './asesores.component.html'
})
export class AlumnoAsesoresTabComponent {
  private readonly evaluacionService = inject(EvaluacionService);

  student = input.required<AlumnoDetailDto>();

  evaluacionesResource = rxResource<EvaluacionDto[], string>({
    params: () => this.student().id,
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
}
