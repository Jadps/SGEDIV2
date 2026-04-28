import { inject, Injectable } from '@angular/core';
import { AlumnoService } from './alumno.service';
import { MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root'
})
export class DocumentActionsService {
  private readonly alumnoService = inject(AlumnoService);
  private readonly messageService = inject(MessageService);

  viewDocument(id: string) {
    this.alumnoService.downloadDocument(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
      },
      error: () => this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No se pudo cargar el documento para visualizar.'
      })
    });
  }

  downloadTemplate(id: number) {
    this.alumnoService.downloadTemplate(id);
  }

  isDeadlineExpired(date: string | Date | null | undefined): boolean {
    if (!date) return false;
    const d = typeof date === 'string' ? new Date(date) : date;
    return d < new Date();
  }
}
