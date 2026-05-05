import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-professor-info-tab',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './info-tab.component.html'
})
export class ProfessorInfoTabComponent {
    data = input.required<any>();
}
