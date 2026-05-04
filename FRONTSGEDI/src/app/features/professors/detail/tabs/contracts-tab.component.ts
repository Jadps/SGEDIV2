import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-professor-contracts-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './contracts-tab.component.html'
})
export class ProfessorContractsTabComponent {
  data = input.required<any>();
}
