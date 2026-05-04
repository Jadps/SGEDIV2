export class DocumentoEstadoUtils {
  static getText(estado: number): string {
    switch (estado) {
      case 0: return 'Pendiente';
      case 1: return 'Aprobado';
      case 2: return 'Rechazado';
      default: return 'No subido';
    }
  }

  static getSeverity(estado: number): string {
    switch (estado) {
      case 0: return 'warn';
      case 1: return 'success';
      case 2: return 'danger';
      default: return 'secondary';
    }
  }

  static mapExpediente(items: any[]): any[] {
    return items.map(doc => ({
      ...doc,
      estadoText: DocumentoEstadoUtils.getText(doc.estado),
      estadoSeverity: DocumentoEstadoUtils.getSeverity(doc.estado)
    }));
  }
}
