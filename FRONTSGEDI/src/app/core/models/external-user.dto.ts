export interface ExternalUserDto {
    id: string;
    name: string;
    email: string;
    empresaId: string;
    empresaNombre: string;
    puesto: string;
    telefonoOficina: string;
    status: number;
}

export interface CreateExternalUserRequest {
    name: string;
    email: string;
    password?: string;
    empresaId: string;
    puesto: string;
    telefonoOficina: string;
}
