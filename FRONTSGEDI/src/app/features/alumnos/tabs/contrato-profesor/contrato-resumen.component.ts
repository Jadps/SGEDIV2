import { Component, input, output, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { TextareaModule } from 'primeng/textarea';
import { ContratoDto } from '../../../../core/models/contrato.dto';
import { ContratoService } from '../../../../core/services/contrato.service';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-contrato-resumen',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TooltipModule,
    DialogModule,
    TextareaModule,
    StatusBadgeComponent
  ],
  templateUrl: './contrato-resumen.component.html'
})
export class ContratoResumenComponent {
  private readonly contratoService = inject(ContratoService);
  private readonly messageService = inject(MessageService);
  private readonly authService = inject(AuthService);

  contrato = input.required<ContratoDto>();
  canRespond = input<boolean>(false);
  onResponded = output<void>();
  onEdit = output<void>();

  showRejectDialog = signal(false);
  observaciones = signal('');
  isSubmitting = signal(false);

  isProfessor = computed(() => this.authService.getUserRoles().some(r => r.toLowerCase() === 'profesor'));
  
  canProfessorEdit = computed(() => {
    const status = this.contrato().estado.toLowerCase();
    return this.isProfessor() && (status === 'pendiente' || status === 'rechazado');
  });

  accept() {
    this.isSubmitting.set(true);
    this.contratoService.respondToContrato(this.contrato().id, true).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Aceptado', detail: 'Has aceptado el contrato de evaluación.' });
        this.onResponded.emit();
        this.isSubmitting.set(false);
      },
      error: () => this.isSubmitting.set(false)
    });
  }

  reject() {
    if (!this.observaciones()) {
      this.messageService.add({ severity: 'warn', summary: 'Atención', detail: 'Por favor indica los motivos del rechazo.' });
      return;
    }

    this.isSubmitting.set(true);
    this.contratoService.respondToContrato(this.contrato().id, false, this.observaciones()).subscribe({
      next: () => {
        this.messageService.add({ severity: 'info', summary: 'Rechazado', detail: 'Has rechazado el contrato de evaluación.' });
        this.showRejectDialog.set(false);
        this.onResponded.emit();
        this.isSubmitting.set(false);
      },
      error: () => this.isSubmitting.set(false)
    });
  }
}
