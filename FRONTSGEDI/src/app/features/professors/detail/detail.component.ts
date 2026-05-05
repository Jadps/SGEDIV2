import { Component, input, output, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { TabsModule } from 'primeng/tabs';
import { ProfessorInfoTabComponent } from './tabs/info/info-tab.component';
import { ProfessorContractsTabComponent } from './tabs/contracts/contracts-tab.component';

@Component({
  selector: 'app-professor-detail',
  standalone: true,
  imports: [CommonModule, DialogModule, TabsModule, ProfessorInfoTabComponent, ProfessorContractsTabComponent],
  templateUrl: './detail.component.html',
})
export class ProfessorDetailComponent {
  visible = input.required<boolean>();
  data = input.required<any>();
  onClose = output<void>();

  activeTab = signal('info');

  onTabChange(event: any) {
    if (this.activeTab() !== event) {
      this.activeTab.set(event);
    }
  }
}
