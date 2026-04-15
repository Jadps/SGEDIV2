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

@Component({
  selector: 'app-alumno-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    InputTextModule,
    IconFieldModule,
    InputIconModule,
    ButtonModule,
    TagModule
  ],
  templateUrl: './list.component.html',
  styleUrl: './list.component.css'
})
export class AlumnoListComponent implements OnInit {
  private readonly alumnoService = inject(AlumnoService);

  public alumnos = signal<AlumnoDto[]>([]);
  public totalRecords = signal<number>(0);
  public loading = signal<boolean>(false);
  public searchTerm = signal<string>('');
  
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
        this.alumnos.set(response.items);
        this.totalRecords.set(response.totalCount);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  onLazyLoad(event: TableLazyLoadEvent) {
    const page = (event.first ?? 0) / (event.rows ?? 10) + 1;
    this.loadAlumnos(page, event.rows ?? 10);
  }

  onSearch(event: Event) {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }
}
