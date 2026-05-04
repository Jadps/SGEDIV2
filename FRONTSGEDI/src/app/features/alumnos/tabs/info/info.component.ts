import { Component, input, signal, inject, computed, output, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';
import { AlumnoDetailDto } from '../../../../core/models/alumno-detail.dto';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { AlumnoService } from '../../../../core/services/alumno.service';
import { CatalogService } from '../../../../core/services/catalog.service';

import { MessageService } from 'primeng/api';
import { StatusUtils } from '../../../../core/utils/status-utils';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { AsesorInternoCatalogDto, AsesorExternoCatalogDto } from '../../../../core/services/catalog.service';

@Component({
  selector: 'app-alumno-info-tab',
  standalone: true,
  imports: [
    CommonModule,
    TagModule,
    ButtonModule,
    StatusBadgeComponent,
    DialogModule,
    SelectModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './info.component.html',
})
export class AlumnoInfoTabComponent {
  private readonly studentService = inject(AlumnoService);
  private readonly catalogService = inject(CatalogService);
  private readonly toast = inject(MessageService);
  private readonly fb = inject(FormBuilder);

  student = input.required<AlumnoDetailDto>();
  statusChanged = output<number>();

  status = signal<number | null>(null);
  isToggling = signal(false);
  isActivating = signal(false);
  showActivationDialog = signal(false);

  companiesResource = rxResource<any[], boolean>({
    params: () => this.showActivationDialog(),
    stream: ({ params: show }) => show ? this.catalogService.getEmpresas() : of([])
  });

  internalAdvisorsResource = rxResource<AsesorInternoCatalogDto[], boolean>({
    params: () => this.showActivationDialog(),
    stream: ({ params: show }) => show ? this.catalogService.getAsesoresInternos() : of([])
  });

  activationForm = this.fb.group({
    companyId: ['', Validators.required],
    internalAdvisorId: ['', Validators.required],
    externalAdvisorId: ['', Validators.required]
  });

  selectedCompanyId = toSignal(this.activationForm.get('companyId')!.valueChanges, { initialValue: '' });

  constructor() {
    effect(() => {
      const companyId = this.selectedCompanyId();
      const control = this.activationForm.get('externalAdvisorId');
      if (companyId) {
        control?.enable();
      } else {
        control?.disable();
      }
    });
  }

  externalAdvisorsResource = rxResource<AsesorExternoCatalogDto[], string | null | undefined>({
    params: () => this.selectedCompanyId(),
    stream: ({ params: companyId }) => companyId ? this.catalogService.getAsesoresExternos(companyId) : of([])
  });

  currentStatus = computed(() => this.status() ?? this.student().status);
  currentStatusText = computed(() => StatusUtils.getText(this.currentStatus()));
  currentStatusSeverity = computed(() => StatusUtils.getSeverity(this.currentStatus(), this.student().isMyCareer));

  toggle() {
    if (this.currentStatus() !== 2) {
      this.showActivationDialog.set(true);
      return;
    }

    this.isToggling.set(true);
    this.studentService.suspendStudent(this.student().id).subscribe({
      next: () => {
        this.status.set(1);
        this.isToggling.set(false);
        this.statusChanged.emit(1);
        this.toast.add({
          severity: 'warn',
          summary: 'Cuenta suspendida',
          detail: `La cuenta de ${this.student().name} fue suspendida.`
        });
      },
      error: () => {
        this.isToggling.set(false);
        this.toast.add({ severity: 'error', summary: 'Error', detail: 'No se pudo suspender la cuenta.' });
      }
    });
  }

  confirmActivation() {
    if (this.activationForm.invalid) return;

    this.isActivating.set(true);
    const val = this.activationForm.value;

    this.studentService.activate({
      alumnoId: this.student().id,
      empresaId: val.companyId!,
      asesorInternoId: val.internalAdvisorId!,
      asesorExternoId: val.externalAdvisorId!
    }).subscribe({
      next: () => {
        this.status.set(2);
        this.isActivating.set(false);
        this.showActivationDialog.set(false);
        this.statusChanged.emit(2);
        this.toast.add({
          severity: 'success',
          summary: 'Cuenta activada',
          detail: `La cuenta de ${this.student().name} fue activada.`
        });
      },
      error: () => {
        this.isActivating.set(false);
        this.toast.add({ severity: 'error', summary: 'Error', detail: 'No se pudo activar la cuenta.' });
      }
    });
  }
}
