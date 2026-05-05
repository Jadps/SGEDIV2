import { Component, input, output, inject, signal, computed, effect, untracked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormArray, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageService } from 'primeng/api';
import { ContratoService } from '../../../../core/services/contrato.service';
import { CreateContratoRequest, CatalogItemDto, ContratoDto } from '../../../../core/models/contrato.dto';
import { toSignal } from '@angular/core/rxjs-interop';
import { startWith } from 'rxjs/operators';

@Component({
  selector: 'app-contrato-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    SelectModule,
    InputNumberModule
  ],
  templateUrl: './contrato-form.component.html'
})
export class ContratoFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly contratoService = inject(ContratoService);
  private readonly messageService = inject(MessageService);

  alumnoId = input.required<string>();
  materiaId = input.required<string>();
  existingContrato = input<ContratoDto | null>(null);
  onSaved = output<void>();

  isSubmitting = signal(false);

  tiposCriterio = signal<CatalogItemDto[]>([]);
  modalidades = signal<CatalogItemDto[]>([]);

  contratoForm = this.fb.group({
    modalidad: [0, Validators.required],
    descripcion: [''],
    criterios: this.fb.array([
      this.createCriterioGroup()
    ])
  });

  private formValue = toSignal(
    this.contratoForm.valueChanges.pipe(startWith(this.contratoForm.value))
  );

  totalPorcentaje = computed(() => {
    const current = this.formValue();
    if (!current?.criterios) return 0;
    return current.criterios.reduce((acc: number, curr: any) => acc + (curr?.porcentaje || 0), 0);
  });

  constructor() {
    this.contratoService.getCatalogs().subscribe(catalogs => {
      this.tiposCriterio.set(catalogs.tiposCriterio);
      this.modalidades.set(catalogs.modalidades);

      if (!this.existingContrato()) {
        if (catalogs.modalidades.length > 0) {
          this.contratoForm.patchValue({ modalidad: catalogs.modalidades[0].value });
        }
        if (catalogs.tiposCriterio.length > 0) {
          const criteria = this.criterios.at(0);
          if (criteria) criteria.patchValue({ tipo: catalogs.tiposCriterio[0].value });
        }
      }
    });

    effect(() => {
      const existing = this.existingContrato();
      if (existing) {
        untracked(() => {
          this.contratoForm.patchValue({
            modalidad: existing.modalidad,
            descripcion: existing.descripcion
          });

          this.criterios.clear();
          existing.criterios.forEach(c => {
            const group = this.fb.group({
              tipo: [c.tipo, Validators.required],
              detalle: [c.detalle, Validators.required],
              porcentaje: [c.porcentaje, [Validators.required, Validators.min(1), Validators.max(100)]]
            });
            this.criterios.push(group);
          });
        });
      }
    });
  }

  get criterios() {
    return this.contratoForm.get('criterios') as FormArray;
  }

  createCriterioGroup() {
    return this.fb.group({
      tipo: [0, Validators.required],
      detalle: ['', Validators.required],
      porcentaje: [0, [Validators.required, Validators.min(1), Validators.max(100)]]
    });
  }

  addCriterio() {
    this.criterios.push(this.createCriterioGroup());
  }

  removeCriterio(index: number) {
    if (this.criterios.length > 1) {
      this.criterios.removeAt(index);
    }
  }

  save() {
    if (this.contratoForm.invalid) {
      this.contratoForm.markAllAsTouched();
      return;
    }

    if (this.totalPorcentaje() !== 100) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'La suma de los porcentajes debe ser 100%.' });
      return;
    }

    this.isSubmitting.set(true);
    const request: CreateContratoRequest = {
      alumnoId: this.alumnoId(),
      materiaId: this.materiaId(),
      modalidad: this.contratoForm.value.modalidad!,
      descripcion: this.contratoForm.value.descripcion!,
      criterios: this.contratoForm.value.criterios as any
    };

    const existing = this.existingContrato();
    const obs = existing 
      ? this.contratoService.updateContrato(existing.id, request)
      : this.contratoService.createContrato(request);

    obs.subscribe({
      next: () => {
        const msg = existing ? 'Contrato actualizado correctamente.' : 'Contrato creado y enviado al alumno.';
        this.messageService.add({ severity: 'success', summary: 'Guardado', detail: msg });
        this.onSaved.emit();
        this.isSubmitting.set(false);
      },
      error: () => this.isSubmitting.set(false)
    });
  }
}
