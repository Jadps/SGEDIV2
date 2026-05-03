export interface AlumnoDetailDto {
    id: string;
    name: string;
    email: string;
    matricula: string;
    carrera: string;
    semestre: string;
    status: number;
    statusText: string;
    statusSeverity: string;
    createdAt: string;

    isMyCareer: boolean;
    isMyStudent: boolean;
    isMyAdvisory: boolean;
}
