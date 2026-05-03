import { Component, input, signal, inject, computed, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { AlumnoDetailDto } from '../../../../core/models/alumno-detail.dto';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { AlumnoService } from '../../../../core/services/alumno.service';
import { MessageService } from 'primeng/api';
import { StatusUtils } from '../../../../core/utils/status-utils';

@Component({
  selector: 'app-alumno-info-tab',
  standalone: true,
  imports: [CommonModule, TagModule, ButtonModule, StatusBadgeComponent],
  templateUrl: './info.component.html',
})
export class AlumnoInfoTabComponent {
  private readonly alumnoService = inject(AlumnoService);
  private readonly toast = inject(MessageService);

  alumno = input.required<AlumnoDetailDto>();
  statusChanged = output<number>();

  status = signal<number | null>(null);
  toggling = signal(false);

  currentStatus = computed(() => this.status() ?? this.alumno().status);

  currentStatusText = computed(() =>
    StatusUtils.getText(this.currentStatus())
  );

  currentStatusSeverity = computed(() =>
    StatusUtils.getSeverity(this.currentStatus(), this.alumno().isMyCareer)
  );

  toggle() {
    this.toggling.set(true);
    this.alumnoService.toggleStatus(this.alumno().id).subscribe({
      next: (res) => {
        this.status.set(res.status);
        this.toggling.set(false);
        this.statusChanged.emit(res.status);
        const isActive = res.status === 2;
        this.toast.add({
          severity: isActive ? 'success' : 'warn',
          summary: isActive ? 'Cuenta activada' : 'Cuenta desactivada',
          detail: `La cuenta de ${this.alumno().name} fue ${isActive ? 'activada' : 'desactivada'}.`
        });
      },
      error: () => {
        this.toggling.set(false);
        this.toast.add({ severity: 'error', summary: 'Error', detail: 'No se pudo cambiar el estado.' });
      }
    });
  }
}