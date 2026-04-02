import { Navbar } from './navbar/navbar';
import { Sidebar } from './sidebar/sidebar';
import { RouterOutlet } from '@angular/router';
import { Component } from '@angular/core';
import { ToastModule } from 'primeng/toast';

@Component({
    selector: 'app-layout',
    standalone: true,
    imports: [RouterOutlet, Navbar, Sidebar, ToastModule],
    templateUrl: './app.layout.html',
})
export class AppLayout { }