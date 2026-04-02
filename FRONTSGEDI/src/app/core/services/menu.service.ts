import { inject, Injectable, signal } from '@angular/core';
import { CatalogService } from './catalog.service';
import { ModuleDto } from '../models/module.dto';
import { tap, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MenuService {
  private readonly catalogService = inject(CatalogService);

  public menuItems = signal<ModuleDto[]>([]);

  loadMenu() {
    return this.catalogService.getMenuModules().pipe(
      tap(modules => {
        this.menuItems.set(modules);
      })
    );
  }

  invalidate() {
    this.menuItems.set([]);
  }

  getAllowedUrls(): string[] {
    const urls: string[] = [];

    const flatten = (modules: ModuleDto[]) => {
      modules.forEach(m => {
        if (m.action) {
          urls.push(m.action);
        }
        if (m.subModules && m.subModules.length > 0) {
          flatten(m.subModules);
        }
      });
    };

    flatten(this.menuItems());
    return urls;
  }
}
