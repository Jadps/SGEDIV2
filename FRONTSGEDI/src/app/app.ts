import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ProgressBar } from 'primeng/progressbar';
import { LoadingService } from './core/services/loading.service';
import { CommonModule } from '@angular/common';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ProgressBar, CommonModule, ToastModule, ConfirmDialogModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  constructor(public loadingService: LoadingService) { }
}
