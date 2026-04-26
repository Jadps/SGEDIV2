import { Injectable } from '@angular/core';

export interface TipoAnexo {
    label: string;
    value: number;
}

export interface ExpectedDocument {
    label: string;
    tipoId: number;
    esAcuerdo: boolean;
}

export type DocumentoEstado = -1 | 0 | 1 | 2;

@Injectable({ providedIn: 'root' })
export class AnexoMetaService {
    readonly tiposPlantilla: TipoAnexo[] = [
        { label: 'Anexo I', value: 1 },
        { label: 'Anexo II', value: 2 },
        { label: 'Anexo III', value: 3 },
        { label: 'Anexo IV', value: 4 },
        { label: 'Anexo V', value: 5 },
        { label: 'Anexo VI', value: 6 },
        { label: 'Anexo VII', value: 7 },
        { label: 'Anexo VIII', value: 8 },
    ];

    readonly expectedDocuments: ExpectedDocument[] = [
        { label: 'Anexo I', tipoId: 1, esAcuerdo: true },
        { label: 'Anexo IV', tipoId: 4, esAcuerdo: true },
        { label: 'Anexo VIII', tipoId: 8, esAcuerdo: true },
        { label: 'Horario', tipoId: 1, esAcuerdo: false },
        { label: 'Kardex', tipoId: 2, esAcuerdo: false },
    ];

    formatTipo(tipo: string | null | undefined): string {
        if (!tipo) return '';
        return tipo.replace(/Anexo([IVX]+)/, 'Anexo $1');
    }

    getSeverity(estado: DocumentoEstado): string {
        const map: Record<DocumentoEstado, string> = {
            1: 'success',
            2: 'danger',
            0: 'warning',
            [-1]: 'secondary',
        };
        return map[estado] ?? 'warning';
    }

    getEstadoText(estado: DocumentoEstado): string {
        const map: Record<DocumentoEstado, string> = {
            1: 'Aprobado',
            2: 'Rechazado',
            0: 'Pendiente',
            [-1]: 'No subido',
        };
        return map[estado] ?? 'Pendiente';
    }

    getAvailableTipos(existingTipos: number[]): TipoAnexo[] {
        return this.tiposPlantilla.filter(t => !existingTipos.includes(t.value));
    }
}