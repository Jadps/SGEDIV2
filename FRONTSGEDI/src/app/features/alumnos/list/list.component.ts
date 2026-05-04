import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AlumnoService } from '../../../core/services/alumno.service';
import { AlumnoDto } from '../../../core/models/alumno.dto';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { AlumnoDetailModalComponent } from '../detail/detail.component';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { StatusUtils } from '../../../core/utils/status-utils';

@Component({
  selector: 'app-alumno-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, InputTextModule,
    IconFieldModule, InputIconModule, ButtonModule, TagModule,
    AlumnoDetailModalComponent, StatusBadgeComponent
  ],
  templateUrl: './list.component.html'
})
export class AlumnoListComponent implements OnInit {
  private readonly alumnoService = inject(AlumnoService);

  alumnos = signal<(AlumnoDto & { statusText: string; statusSeverity: string })[]>([]);
  totalRecords = signal<number>(0);
  loading = signal<boolean>(false);
  searchTerm = signal<string>('');

  selectedAlumnoId = signal<string | null>(null);
  showModal = signal<boolean>(false);

  private searchSubject = new Subject<string>();

  ngOnInit() {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(term => {
      this.searchTerm.set(term);
      this.loadAlumnos(1);
    });
  }

  loadAlumnos(page: number = 1, pageSize: number = 10) {
    this.loading.set(true);
    this.alumnoService.getAlumnos(page, pageSize, this.searchTerm()).subscribe({
      next: (response) => {
        this.alumnos.set(response.items.map(a => ({
          ...a,
          statusText: StatusUtils.getText(a.status),
          statusSeverity: StatusUtils.getSeverity(a.status)
        })));
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onLazyLoad(event: TableLazyLoadEvent) {
    const page = (event.first ?? 0) / (event.rows ?? 10) + 1;
    this.loadAlumnos(page, event.rows ?? 10);
  }

  onSearch(event: Event) {
    this.searchSubject.next((event.target as HTMLInputElement).value);
  }

  openDetail(alumnoId: string) {
    this.selectedAlumnoId.set(alumnoId);
    this.showModal.set(true);
  }

  closeDetail() {
    this.showModal.set(false);
    this.selectedAlumnoId.set(null);
  }

  onStatusChanged(alumnoId: string, status: number) {
    this.alumnos.update(list =>
      list.map(a => a.id === alumnoId
        ? {
          ...a,
          status: status,
          statusText: StatusUtils.getText(status),
          statusSeverity: StatusUtils.getSeverity(status)
        }
        : a
      )
    );
  }
}