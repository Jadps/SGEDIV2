export class StatusUtils {
  static getText(isActive: boolean): string {
    return isActive ? 'Activo' : 'Cuenta Inactiva';
  }

  static getSeverity(isActive: boolean, isMyCareer: boolean = true): string {
    if (!isActive) {
      return isMyCareer ? 'danger' : 'secondary';
    }
    return 'success';
  }
}
