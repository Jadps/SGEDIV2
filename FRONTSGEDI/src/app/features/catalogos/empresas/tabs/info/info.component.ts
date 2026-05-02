import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { EmpresaDto } from '../../../../../core/models/empresa.dto';

@Component({
  selector: 'app-empresa-info-tab',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, ButtonModule],
  templateUrl: './info.component.html'
})
export class EmpresaInfoTabComponent {
  empresa = input.required<EmpresaDto>();
  onSave = output<EmpresaDto>();

  save() {
    this.onSave.emit(this.empresa());
  }
}
