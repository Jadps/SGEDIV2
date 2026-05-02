import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CatalogService } from '../../../core/services/catalog.service';
import { EmpresaDto } from '../../../core/models/empresa.dto';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { NotificationService } from '../../../core/services/notification.service';
import { EmpresaDetailModalComponent } from './detail/detail.component';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { TooltipModule } from 'primeng/tooltip';
import { computed } from '@angular/core';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-empresas',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    TableModule, 
    ButtonModule, 
    InputTextModule, 
    DialogModule, 
    EmpresaDetailModalComponent,
    IconFieldModule,
    InputIconModule,
    TooltipModule
  ],
  templateUrl: './empresas.component.html'
})
export class EmpresasComponent implements OnInit {
  private readonly catalogService = inject(CatalogService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);

  empresas = signal<EmpresaDto[]>([]);
  loading = signal<boolean>(false);
  searchTerm = signal<string>('');
  
  displayDetail = signal<boolean>(false);
  selectedEmpresaId = signal<string | null>(null);

  filteredEmpresas = computed(() => {
    const term = this.searchTerm().toLowerCase();
    if (!term) return this.empresas();
    return this.empresas().filter(e => 
      e.nombre.toLowerCase().includes(term) || 
      e.rfc.toLowerCase().includes(term) ||
      e.correo.toLowerCase().includes(term)
    );
  });

  ngOnInit() {
    this.loadEmpresas();
  }

  loadEmpresas() {
    this.loading.set(true);
    this.catalogService.getEmpresas().subscribe({
      next: (data) => {
        this.empresas.set(data);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  openNew() {
    this.selectedEmpresaId.set(null);
    this.displayDetail.set(true);
  }

  editEmpresa(empresa: EmpresaDto) {
    this.selectedEmpresaId.set(empresa.id);
    this.displayDetail.set(true);
  }


  deleteEmpresa(id: string) {
    this.confirmationService.confirm({
      message: '¿Estás seguro de eliminar esta empresa?',
      header: 'Confirmar Eliminación',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.catalogService.deleteEmpresa(id).subscribe({
          next: () => {
            this.notificationService.success('Empresa eliminada', 'La empresa se ha eliminado correctamente');
            this.loadEmpresas();
          }
        });
      }
    });
  }

  onSearch(event: any) {
    this.searchTerm.set(event.target.value);
  }
}
