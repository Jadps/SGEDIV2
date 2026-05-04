import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-professor-info-tab',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-6 space-y-6">
        <div class="space-y-4">
            <h3 class="text-xs font-semibold uppercase tracking-wider text-zinc-500">Subject Information</h3>
            <div class="grid grid-cols-2 gap-4">
                <div class="flex flex-col gap-1">
                    <span class="text-[11px] text-zinc-500 uppercase tracking-wider">Key / Code</span>
                    <span class="text-blue-400 font-mono font-medium">{{ data().materiaClave }}</span>
                </div>
                <div class="flex flex-col gap-1">
                    <span class="text-[11px] text-zinc-500 uppercase tracking-wider">Credits</span>
                    <span class="text-zinc-200">{{ data().materiaCreditos }} credits</span>
                </div>
            </div>
        </div>

        <div class="space-y-4">
            <h3 class="text-xs font-semibold uppercase tracking-wider text-zinc-500">Professor Information</h3>
            <div class="bg-white/5 p-4 rounded-2xl border border-white/5 space-y-3">
                <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-full bg-zinc-800 flex items-center justify-center text-zinc-400">
                        <i class="pi pi-user"></i>
                    </div>
                    <div>
                        <div class="text-zinc-200 font-medium">{{ data().profesorNombre }}</div>
                        <div class="text-zinc-500 text-xs">{{ data().profesorEmail }}</div>
                    </div>
                </div>
                <div class="flex flex-col gap-1 mt-2">
                    <span class="text-[11px] text-zinc-500 uppercase tracking-wider">Employee Number</span>
                    <span class="text-zinc-300 text-sm">{{ data().profesorNumeroEmpleado || 'N/A' }}</span>
                </div>
            </div>
        </div>
    </div>
  `
})
export class ProfessorInfoTabComponent {
  data = input.required<any>();
}
