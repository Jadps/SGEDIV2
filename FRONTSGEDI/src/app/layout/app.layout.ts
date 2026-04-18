import { Navbar } from './navbar/navbar';
import { Sidebar } from './sidebar/sidebar';
import { RouterOutlet } from '@angular/router';
import { Component } from '@angular/core';

@Component({
    selector: 'app-layout',
    standalone: true,
    imports: [RouterOutlet, Navbar, Sidebar],
    templateUrl: './app.layout.html',
})
export class AppLayout { }