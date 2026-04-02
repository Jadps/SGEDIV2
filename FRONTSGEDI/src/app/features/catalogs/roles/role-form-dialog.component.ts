import { Component, inject, ChangeDetectionStrategy, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, Validators } from '@angular/forms';
import { TreeTableModule } from 'primeng/treetable';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { DialogModule } from 'primeng/dialog';
import { RoleManagerService } from './role-manager.service';
import { TreeNode, SharedModule } from 'primeng/api';
import { RoleDto } from '../../../core/models/role.dto';
import { ModuleDto } from '../../../core/models/module.dto';

@Component({
    selector: 'app-role-form-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        TreeTableModule,
        CheckboxModule,
        ButtonModule,
        InputTextModule,
        TextareaModule,
        DialogModule,
        SharedModule
    ],
    template: `
        <p-dialog 
            [header]="manager.editingRole() ? 'Editar Rol' : 'Nuevo Rol'" 
            [visible]="manager.isDialogVisible()" 
            [modal]="true" 
            [style]="{ width: '70vw' }"
            (onHide)="onClose()">
            
            <form [formGroup]="roleForm" class="flex flex-col gap-4">
                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div class="flex flex-col gap-2">
                        <label for="name" class="font-bold">Nombre del Rol</label>
                        <input pInputText id="name" formControlName="name" placeholder="Ej: Administrador" />
                    </div>
                    <div class="flex flex-col gap-2">
                        <label for="description" class="font-bold">Descripción</label>
                        <textarea pInputTextarea id="description" formControlName="description" rows="1" placeholder="Opcional.."></textarea>
                    </div>
                </div>

                <div class="flex flex-col gap-3">
                    <h3 class="text-lg font-bold border-b pb-2">Matriz de Permisos por Módulo</h3>
                    
                    <p-treeTable [value]="moduleTree()" [columns]="cols" styleClass="p-datatable-sm p-datatable-gridlines">
                        <ng-template pTemplate="header" let-columns>
                            <tr>
                                <th *ngFor="let col of columns">
                                    {{ col.header }}
                                </th>
                            </tr>
                        </ng-template>
                        <ng-template pTemplate="body" let-rowNode let-rowData="rowData">
                            <tr>
                                <td>
                                    <p-treeTableToggler [rowNode]="rowNode"></p-treeTableToggler>
                                    {{ rowData.description }}
                                </td>
                                <td class="text-center w-24">
                                    <div class="flex flex-col items-center gap-1">
                                        <p-checkbox 
                                            [binary]="true" 
                                            [ngModel]="getPermission(rowData.id) >= 1" 
                                            [ngModelOptions]="{standalone: true}"
                                            (onChange)="toggle(rowData.id, 1)">
                                        </p-checkbox>
                                        <span class="text-xs text-gray-500">Lectura</span>
                                    </div>
                                </td>
                                <td class="text-center w-24">
                                    <div class="flex flex-col items-center gap-1">
                                        <p-checkbox 
                                            [binary]="true" 
                                            [ngModel]="getPermission(rowData.id) >= 2" 
                                            [ngModelOptions]="{standalone: true}"
                                            [disabled]="rowData.moduleTypeId === 1"
                                            (onChange)="toggle(rowData.id, 2)">
                                        </p-checkbox>
                                        <span class="text-xs text-gray-500" [class.opacity-50]="rowData.moduleTypeId === 1">Escritura</span>
                                    </div>
                                </td>
                                <td class="text-center w-24">
                                    <div class="flex flex-col items-center gap-1">
                                        <p-checkbox 
                                            [binary]="true" 
                                            [ngModel]="getPermission(rowData.id) >= 3" 
                                            [ngModelOptions]="{standalone: true}"
                                            [disabled]="rowData.moduleTypeId === 1"
                                            (onChange)="toggle(rowData.id, 3)">
                                        </p-checkbox>
                                        <span class="text-xs text-gray-500" [class.opacity-50]="rowData.moduleTypeId === 1">Admin</span>
                                    </div>
                                </td>
                            </tr>
                        </ng-template>
                    </p-treeTable>
                </div>

                <div class="flex justify-end gap-3 mt-4 pt-4 border-t">
                    <button pButton label="Cancelar" icon="pi pi-times" class="p-button-text" (click)="onClose()"></button>
                    <button pButton label="Guardar Rol" icon="pi pi-check" [loading]="manager.isSaving()" (click)="onSave()" [disabled]="roleForm.invalid"></button>
                </div>
            </form>
        </p-dialog>
    `,
    styles: [`
        :host ::ng-deep .p-treetable-toggler {
            margin-right: 0.5rem;
        }
    `],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class RoleFormDialogComponent {
    private readonly fb = inject(FormBuilder);
    readonly manager = inject(RoleManagerService);

    roleForm = this.fb.group({
        name: ['', [Validators.required, Validators.minLength(3)]],
        description: ['']
    });

    cols = [
        { field: 'description', header: 'Módulo' },
        { field: 'read', header: 'Lectura' },
        { field: 'write', header: 'Escritura' },
        { field: 'admin', header: 'Admin' }
    ];

    moduleTree = computed(() => {
        const modules = this.manager.modules();
        return this.buildTree(modules);
    });

    constructor() {
        effect(() => {
            const role = this.manager.editingRole();
            if (role) {
                this.roleForm.patchValue({
                    name: role.name,
                    description: role.description
                });
            } else {
                this.roleForm.reset();
            }
        });
    }

    getPermission(id: string): number {
        return this.manager.getPermissionLevel(id);
    }

    toggle(id: string, level: number) {
        this.manager.togglePermission(id, level);
    }

    onClose() {
        this.manager.closeDialog();
    }

    async onSave() {
        if (this.roleForm.invalid) return;

        const roleData = this.roleForm.value as RoleDto;
        if (this.manager.editingRole()) {
            await this.manager.updateRole(roleData);
        } else {
            await this.manager.createRole(roleData);
        }
    }

    private buildTree(modules: ModuleDto[]): TreeNode[] {
        return modules.map(m => ({
            data: {
                id: m.id,
                description: m.description,
                moduleTypeId: m.moduleTypeId
            },
            children: m.subModules ? this.buildTree(m.subModules) : [],
            expanded: true
        }));
    }
}
