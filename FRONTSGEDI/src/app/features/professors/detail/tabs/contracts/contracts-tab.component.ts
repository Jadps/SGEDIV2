import { Component, inject, signal, input, computed, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { ContratoService } from '../../../../../core/services/contrato.service';
import { ContratoDto } from '../../../../../core/models/contrato.dto';
import { ContratoResumenComponent } from '../../../../alumnos/tabs/contrato-profesor/contrato-resumen.component';
import { AlumnoService } from '../../../../../core/services/alumno.service';
import { AuthService } from '../../../../../core/services/auth.service';

@Component({
  selector: 'app-professor-contracts-tab',
  standalone: true,
  imports: [CommonModule, ButtonModule, ContratoResumenComponent],
  templateUrl: './contracts-tab.component.html'
})
export class ProfessorContractsTabComponent {
  private readonly contratoService = inject(ContratoService);
  private readonly alumnoService = inject(AlumnoService);
  private readonly authService = inject(AuthService);

  data = input.required<any>();
  contratoActual = signal<ContratoDto | null>(null);
  loading = signal(false);
  alumnoId = signal<string | null>(null);

  isStudent = computed(() => this.authService.getUserRoles().some(r => r.toLowerCase() === 'alumno'));

  constructor() {
    this.alumnoService.getMyProfile().subscribe(profile => {
      this.alumnoId.set(profile.id);
    });

    effect(() => {
      if (this.alumnoId() && this.data()?.materiaId) {
        untracked(() => this.loadContrato());
      }
    });
  }

  loadContrato() {
    const aid = this.alumnoId();
    const mid = this.data()?.materiaId;
    if (!aid || !mid) return;

    this.loading.set(true);
    this.contratoService.getContrato(aid, mid).subscribe({
      next: (contrato) => {
        this.contratoActual.set(contrato);
        this.loading.set(false);
      },
      error: () => {
        this.contratoActual.set(null);
        this.loading.set(false);
      }
    });
  }
}
