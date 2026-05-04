import { Component, input, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExpedienteService } from '../../../../core/services/expediente.service';
import { DocumentUploadService } from '../../../../core/services/document-upload.service';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { rxResource } from '@angular/core/rxjs-interop';
import { of } from 'rxjs';
import { map } from 'rxjs';
import { DocumentoEstadoUtils } from '../../../../core/utils/documento-estado-utils';

@Component({
  selector: 'app-professor-contracts-tab',
  standalone: true,
  imports: [CommonModule, ButtonModule, TagModule],
  template: `
    <div class="p-6 space-y-4">
        @if (expedienteResource.isLoading()) {
            <div class="flex justify-center py-10">
                <i class="pi pi-spin pi-spinner text-2xl text-zinc-500"></i>
            </div>
        } @else {
            @for (doc of filteredDocs(); track doc.documentoId) {
                <div class="flex items-center justify-between p-4 bg-zinc-900/40 rounded-2xl border border-white/5 hover:border-white/10 transition-all">
                    <div class="flex items-center gap-4">
                        <div class="w-10 h-10 rounded-xl bg-red-500/10 flex items-center justify-center text-red-400">
                            <i class="pi pi-file-pdf text-xl"></i>
                        </div>
                        <div>
                            <div class="text-zinc-200 font-medium">{{ doc.label }}</div>
                            <div class="text-zinc-500 text-xs">{{ doc.archivo?.fechaSubida | date:'dd/MM/yyyy HH:mm' }}</div>
                        </div>
                    </div>
                    <div class="flex items-center gap-3">
                        <p-tag [value]="doc.estadoText" [severity]="doc.estadoSeverity" />
                        @if (doc.archivo) {
                            <p-button icon="pi pi-download" [outlined]="true" size="small" (click)="download(doc.documentoId)" />
                        }
                    </div>
                </div>
            } @empty {
                <div class="py-10 text-center space-y-3">
                    <i class="pi pi-file-excel text-4xl text-zinc-700"></i>
                    <p class="text-zinc-500 italic">No contracts found for this subject.</p>
                </div>
            }
        }
    </div>
  `
})
export class ProfessorContractsTabComponent {
  private readonly expedienteService = inject(ExpedienteService);
  private readonly uploadService = inject(DocumentUploadService);
  
  data = input.required<any>();

  expedienteResource = rxResource<any[], string>({
    params: () => this.data().semestre,
    stream: ({ params: semestre }) => semestre
      ? this.expedienteService.getExpediente('me', semestre).pipe(
          map(docs => DocumentoEstadoUtils.mapExpediente(docs))
        )
      : of([])
  });

  filteredDocs = computed(() => {
    const docs = this.expedienteResource.value() as any[] || [];
    return docs.filter((d: any) => d.profesorId === this.data().profesorId);
  });

  download(docId: string) {
    this.uploadService.downloadDocument(docId).subscribe(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `contract_${docId}.pdf`;
        a.click();
    });
  }
}
