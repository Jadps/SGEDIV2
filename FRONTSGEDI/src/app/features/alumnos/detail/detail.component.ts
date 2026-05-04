import { Component, inject, input, output, signal, effect, untracked, computed } from '@angular/core';

import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { TabsModule } from 'primeng/tabs';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AlumnoService } from '../../../core/services/alumno.service';
import { AlumnoDetailDto } from '../../../core/models/alumno-detail.dto';
import { AlumnoInfoTabComponent } from '../tabs/info/info.component';
import { AlumnoDocsTabComponent } from '../tabs/docs/docs.component';
import { AlumnoMateriaDocsTabComponent } from '../tabs/materia-docs/materia-docs.component';

@Component({
  selector: 'app-alumno-detail-modal',
  standalone: true,
  imports: [
    CommonModule, DialogModule, TabsModule,
    ProgressSpinnerModule, AlumnoInfoTabComponent,
    AlumnoDocsTabComponent, AlumnoMateriaDocsTabComponent
  ],
  templateUrl: './detail.component.html',
})
export class AlumnoDetailModalComponent {

  private readonly alumnoService = inject(AlumnoService);

  alumnoId = input.required<string>();
  visible = input.required<boolean>();
  onClose = output<void>();
  statusChanged = output<number>();

  alumno = signal<AlumnoDetailDto | null>(null);
  loading = signal(false);
  activeTab = signal<string>('info');

  isCoordinatorOrAdmin = computed(() => {
    const a = this.alumno();
    return !!(a?.isAdmin || a?.isMyCareer);
  });

  isProfessorOnly = computed(() => {
    const a = this.alumno();
    return !!(a?.isMyStudent && !this.isCoordinatorOrAdmin());
  });

  constructor() {
    effect(() => {
      const isVisible = this.visible();
      const id = this.alumnoId();

      if (isVisible && id) {
        untracked(() => this.fetch());
      }
    });
  }


  onTabChange(event: string | number | undefined) {
    if (event !== undefined) {
      this.activeTab.set(event as string);
    }
  }


  private fetch() {
    this.loading.set(true);
    this.alumnoService.getAlumno(this.alumnoId()).subscribe({
      next: (data: AlumnoDetailDto) => {
        this.alumno.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }
}