import { inject, Injectable } from '@angular/core';
import { HttpContext } from '@angular/common/http';
import { DocumentUploadService } from './document-upload.service';
import { MessageService } from 'primeng/api';
import { SKIP_ERROR_NOTIFICATION } from '../constants/http-context';


@Injectable({
  providedIn: 'root'
})
export class DocumentActionsService {
  private readonly uploadService = inject(DocumentUploadService);
  private readonly messageService = inject(MessageService);

  viewDocument(id: string) {
    const context = new HttpContext().set(SKIP_ERROR_NOTIFICATION, true);
    this.uploadService.downloadDocument(id, context).subscribe({

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
    const context = new HttpContext().set(SKIP_ERROR_NOTIFICATION, true);
    this.uploadService.downloadTemplate(id, context).subscribe({

      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `Plantilla_${id}.docx`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: () => this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'No se pudo descargar la plantilla.'
      })
    });
  }


  isDeadlineExpired(date: string | Date | null | undefined): boolean {
    if (!date) return false;
    const d = typeof date === 'string' ? new Date(date) : date;
    return d < new Date();
  }
}
