export interface InternalUserDto {
    id: string;
    name: string;
    email: string;
    roles: string[];
    isActive: boolean;
    carreraId?: number;
    carreraNombre?: string;
    numeroEmpleado?: string;
    cubiculo?: string;
}

export interface CreateInternalUserRequest {
    name: string;
    email: string;
    password?: string;
    roles: string[];
    carreraId?: number;
    numeroEmpleado?: string;
    cubiculo?: string;
}
