import { Component, input, output, signal, computed, HostListener, model } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-file-uploader',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-uploader.component.html',
  styleUrl: './file-uploader.component.css'
})
export class FileUploaderComponent {
  id = input<string>('file-upload-' + Math.random().toString(36).substring(2, 9));
  label = input<string>('Haz clic para subir');
  description = input<string>('PDF, DOCX (Máx. 10MB)');
  accept = input<string>('.doc,.docx,.pdf');
  icon = input<string>('pi-cloud-upload');
  color = input<'blue' | 'purple' | 'emerald' | 'amber'>('blue');

  fileSelected = output<File | null>();

  selectedFile = model<File | null>(null);
  isDragging = signal(false);

  containerClasses = computed(() => {
    const base = 'border-white/10 group-hover:bg-zinc-800/50';
    switch (this.color()) {
      case 'purple': return `${base} group-hover:border-purple-500/50`;
      case 'emerald': return `${base} group-hover:border-emerald-500/50`;
      case 'amber': return `${base} group-hover:border-amber-500/50`;
      default: return `${base} group-hover:border-blue-500/50`;
    }
  });

  iconClass = computed(() => {
    const icon = this.icon();
    switch (this.color()) {
      case 'purple': return `pi ${icon} text-zinc-500 group-hover:text-purple-400`;
      case 'emerald': return `pi ${icon} text-zinc-500 group-hover:text-emerald-400`;
      case 'amber': return `pi ${icon} text-zinc-500 group-hover:text-amber-400`;
      default: return `pi ${icon} text-zinc-500 group-hover:text-blue-400`;
    }
  });

  labelClass = computed(() => {
    switch (this.color()) {
      case 'purple': return 'font-semibold text-purple-400';
      case 'emerald': return 'font-semibold text-emerald-400';
      case 'amber': return 'font-semibold text-amber-400';
      default: return 'font-semibold text-blue-400';
    }
  });

  handleFileChange(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile.set(file);
      this.fileSelected.emit(file);
    }
  }

  clearFile() {
    this.selectedFile.set(null);
    this.fileSelected.emit(null);
  }

  @HostListener('dragover', ['$event'])
  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  @HostListener('dragleave', ['$event'])
  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  @HostListener('drop', ['$event'])
  onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const file = event.dataTransfer?.files[0];
    if (file) {
      this.selectedFile.set(file);
      this.fileSelected.emit(file);
    }
  }
}
