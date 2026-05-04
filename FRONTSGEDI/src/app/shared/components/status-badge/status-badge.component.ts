import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.css'
})
export class StatusBadgeComponent {
  severity = input.required<string>();
  text = input.required<string>();

  textClasses = computed(() => {
    const s = this.severity();
    switch (s) {
      case 'success': return 'text-emerald-500';
      case 'danger': return 'text-red-400';
      case 'warning': return 'text-amber-400';
      default: return 'text-zinc-400';
    }
  });

  dotClasses = computed(() => {
    const s = this.severity();
    switch (s) {
      case 'success': return 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.4)]';
      case 'danger': return 'bg-red-500 shadow-[0_0_8px_rgba(239,68,68,0.4)]';
      case 'warning': return 'bg-amber-400 shadow-[0_0_8px_rgba(251,191,36,0.4)]';
      default: return 'bg-zinc-500';
    }
  });
}
