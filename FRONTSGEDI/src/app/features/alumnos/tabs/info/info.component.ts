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
  statusChanged = output<boolean>();

  isActive = signal<boolean | null>(null);
  toggling = signal(false);

  currentIsActive = computed(() => this.isActive() ?? this.alumno().isAccountActive);

  currentStatusText = computed(() =>
    StatusUtils.getText(this.currentIsActive())
  );

  currentStatusSeverity = computed(() =>
    StatusUtils.getSeverity(this.currentIsActive(), this.alumno().isMyCareer)
  );

  toggle() {
    this.toggling.set(true);
    this.alumnoService.toggleStatus(this.alumno().id).subscribe({
      next: (res) => {
        this.isActive.set(res.isActive);
        this.toggling.set(false);
        this.statusChanged.emit(res.isActive);
        this.toast.add({
          severity: res.isActive ? 'success' : 'warn',
          summary: res.isActive ? 'Cuenta activada' : 'Cuenta desactivada',
          detail: `La cuenta de ${this.alumno().name} fue ${res.isActive ? 'activada' : 'desactivada'}.`
        });
      },
      error: () => {
        this.toggling.set(false);
        this.toast.add({ severity: 'error', summary: 'Error', detail: 'No se pudo cambiar el estado.' });
      }
    });
  }
}