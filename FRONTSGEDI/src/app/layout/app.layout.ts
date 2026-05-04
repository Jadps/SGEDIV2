import { Navbar } from './navbar/navbar';
import { Sidebar } from './sidebar/sidebar';
import { RouterOutlet } from '@angular/router';
import { Component, inject } from '@angular/core';
import { LayoutService } from '../core/services/layout.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-layout',
    standalone: true,
    imports: [RouterOutlet, Navbar, Sidebar, CommonModule],
    templateUrl: './app.layout.html',
})
export class AppLayout {
    layoutService = inject(LayoutService);
}