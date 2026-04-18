import { Component, inject, input, output, signal, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { TabsModule } from 'primeng/tabs';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AlumnoService } from '../../../core/services/alumno.service';
import { AlumnoDetailDto } from '../../../core/models/alumno-detail.dto';
import { AlumnoInfoTabComponent } from '../tabs/info/info.component';

@Component({
  selector: 'app-alumno-detail-modal',
  standalone: true,
  imports: [
    CommonModule, DialogModule, TabsModule,
    ProgressSpinnerModule, AlumnoInfoTabComponent
  ],
  templateUrl: './detail.component.html',
})
export class AlumnoDetailModalComponent implements OnChanges {
  private readonly alumnoService = inject(AlumnoService);

  alumnoId = input.required<string>();
  visible = input.required<boolean>();
  onClose = output<void>();
  statusChanged = output<boolean>();

  alumno = signal<AlumnoDetailDto | null>(null);
  loading = signal(false);

  ngOnChanges() {
    if (this.visible() && this.alumnoId()) {
      this.fetch();
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