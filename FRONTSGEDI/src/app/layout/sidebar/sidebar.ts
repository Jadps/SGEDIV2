import { Component, inject, signal, effect, computed } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';

import { AuthService } from '../../core/services/auth.service';
import { MenuService } from '../../core/services/menu.service';
import { ModuleDto } from '../../core/models/module.dto';

export interface SidebarModule extends ModuleDto {
  expanded?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  public authService = inject(AuthService);
  private menuService = inject(MenuService);
  private router = inject(Router);

  private expandedModuleIds = signal<Set<string>>(new Set());

  public menuItems = computed(() => {
    const modules = this.menuService.menuItems();
    const expandedIds = this.expandedModuleIds();
    const currentUrl = this.router.url;

    return modules.map(m => {
      const id = m.id || '';
      const hasActiveChild = m.subModules?.some(sm => sm.action && currentUrl.includes(sm.action));
      const isExpanded = expandedIds.has(id) || hasActiveChild;

      return {
        ...m,
        expanded: isExpanded
      } as SidebarModule;
    });
  });

  constructor() {
    effect(() => {
      const user = this.authService.currentUser();
      const menuEmpty = this.menuService.menuItems().length === 0;

      if (user && menuEmpty) {
        this.menuService.loadMenu().subscribe();
      }
    });
  }

  toggleModule(moduleId: string) {
    this.expandedModuleIds.update(set => {
      const newSet = new Set(set);
      if (newSet.has(moduleId)) {
        newSet.delete(moduleId);
      } else {
        newSet.add(moduleId);
      }
      return newSet;
    });
  }
}