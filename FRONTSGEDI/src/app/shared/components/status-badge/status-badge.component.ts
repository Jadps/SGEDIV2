import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center gap-2 py-1 px-2.5 rounded-full glass-dark border border-white/5 w-fit">
      <div class="w-1.5 h-1.5 rounded-full animate-pulse-slow" [ngClass]="dotClasses()"></div>
      <span class="text-[10px] font-semibold tracking-wider uppercase transition-colors" [ngClass]="textClasses()">
        {{ text() }}
      </span>
    </div>
  `,
  styles: [`
    @keyframes pulse-slow {
      0%, 100% { opacity: 1; transform: scale(1); }
      50% { opacity: 0.7; transform: scale(0.95); }
    }
    .animate-pulse-slow {
      animation: pulse-slow 3s cubic-bezier(0.4, 0, 0.6, 1) infinite;
    }
  `]
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
