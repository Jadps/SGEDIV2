export interface AlumnoDetailDto {
    id: string;
    name: string;
    email: string;
    matricula: string;
    carrera: string;
    semestre: string;
    isAccountActive: boolean;
    statusText: string;
    statusSeverity: string;
    createdAt: string;

    isMyCareer: boolean;
    isMyStudent: boolean;
    isMyAdvisory: boolean;
}
