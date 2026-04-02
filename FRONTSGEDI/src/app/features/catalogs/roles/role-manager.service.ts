import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { API_ENDPOINTS } from '../../../core/constants/api-endpoints';
import { MessageService } from 'primeng/api';
import { RoleDto } from '../../../core/models/role.dto';
import { ModuleDto } from '../../../core/models/module.dto';
import { MenuService } from '../../../core/services/menu.service';

@Injectable({
    providedIn: 'root'
})
export class RoleManagerService {
    private readonly http = inject(HttpClient);
    private readonly messageService = inject(MessageService);
    private readonly menuService = inject(MenuService);
    private readonly apiUrl = `${environment.apiUrl}${API_ENDPOINTS.CATALOGS.ROLES}`;

    readonly roles = signal<RoleDto[]>([]);
    readonly modules = signal<ModuleDto[]>([]);
    readonly selectedPermissions = signal<Map<string, number>>(new Map());

    readonly isTableLoading = signal<boolean>(false);
    readonly isSaving = signal<boolean>(false);
    readonly isDialogVisible = signal<boolean>(false);
    readonly editingRole = signal<RoleDto | null>(null);

    async openNewRoleDialog() {
        this.editingRole.set(null);
        this.selectedPermissions.set(new Map());
        await this.ensureModulesLoaded();
        this.isDialogVisible.set(true);
    }

    async openEditRoleDialog(role: RoleDto) {
        this.editingRole.set(role);

        const permissionsMap = new Map<string, number>();
        if (role.permissions) {
            role.permissions.forEach((p) => {
                permissionsMap.set(p.moduleId, p.permission);
            });
        }
        this.selectedPermissions.set(permissionsMap);

        await this.ensureModulesLoaded();
        this.isDialogVisible.set(true);
    }

    closeDialog() {
        this.isDialogVisible.set(false);
        this.editingRole.set(null);
        this.selectedPermissions.set(new Map());
    }

    private async ensureModulesLoaded() {
        if (this.modules().length === 0) {
            await this.loadModulesForPermissions();
        }
    }

    async loadRoles() {
        this.isTableLoading.set(true);
        try {
            const response = await firstValueFrom(this.http.get<any>(`${this.apiUrl}`));
            const data = response.data || response || [];
            this.roles.set(data);
        } catch (error) {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al cargar roles' });
            console.error('Error loading roles:', error);
        } finally {
            this.isTableLoading.set(false);
        }
    }

    async loadModulesForPermissions() {
        try {
            const response = await firstValueFrom(this.http.get<any>(`${environment.apiUrl}${API_ENDPOINTS.CATALOGS.MODULES}`));
            this.modules.set(response.data || response || []);
        } catch (error) {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al cargar módulos' });
            console.error('Error loading modules:', error);
        }
    }

    async createRole(role: RoleDto) {
        this.isSaving.set(true);
        try {
            const permissions = Array.from(this.selectedPermissions().entries()).map(([moduleId, permission]) => ({
                moduleId,
                permission
            }));

            await firstValueFrom(this.http.post(`${this.apiUrl}`, { ...role, permissions }));
            this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Rol creado correctamente' });
            this.closeDialog();
            this.menuService.invalidate();
            await this.loadRoles();
        } catch (error) {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al crear rol' });
            console.error('Error creating role:', error);
        } finally {
            this.isSaving.set(false);
        }
    }

    async updateRole(role: RoleDto) {
        this.isSaving.set(true);
        try {
            const permissions = Array.from(this.selectedPermissions().entries()).map(([moduleId, permission]) => ({
                moduleId,
                permission
            }));

            const rolePayload = { ...role, id: this.editingRole()?.id, permissions };
            await firstValueFrom(this.http.put(`${this.apiUrl}`, rolePayload));
            this.messageService.add({ severity: 'success', summary: 'Éxito', detail: 'Rol actualizado correctamente' });
            this.closeDialog();
            this.menuService.invalidate();
            await this.loadRoles();
        } catch (error) {
            this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Error al actualizar rol' });
            console.error('Error updating role:', error);
        } finally {
            this.isSaving.set(false);
        }
    }

    togglePermission(moduleId: string, level: number) {
        const current = new Map(this.selectedPermissions());
        const currentLevel = current.get(moduleId) || 0;

        if (currentLevel === level) {
            current.set(moduleId, 0);
        } else {
            current.set(moduleId, level);
        }

        this.selectedPermissions.set(current);
    }

    getPermissionLevel(moduleId: string): number {
        return this.selectedPermissions().get(moduleId) || 0;
    }
}
