export interface RolePermissionDto {
    moduleId: string;
    permission: number;
}

export interface RoleDto {
    id?: string;
    name: string;
    description?: string | null;
    tenantId?: string | null;
    permissions?: RolePermissionDto[];
}
