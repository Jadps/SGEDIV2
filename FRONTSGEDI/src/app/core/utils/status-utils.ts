export class StatusUtils {
  static getText(status: number): string {
    switch (status) {
      case 1: return 'Sin Activar';
      case 2: return 'Activo';
      case 3: return 'Borrado';
      default: return 'Desconocido';
    }
  }

  static getSeverity(status: number, isMyCareer: boolean = true): string {
    switch (status) {
      case 1: return 'warn';
      case 2: return 'success';
      case 3: return 'danger';
      default: return 'secondary';
    }
  }
}
