import { Directive, HostBinding, HostListener, Output, EventEmitter } from '@angular/core';

@Directive({
  selector: '[appDrag]'
})

export class DragDropDirective {
  @Output() file: EventEmitter<File> = new EventEmitter();

  @HostBinding('style.background') private background = '#eee';

  @HostListener('dragover', ['$event']) public onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.background = '#999';
  }

  @HostListener('dragleave', ['$event']) public onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.background = '#eee';
  }

  @HostListener('drop', ['$event']) public onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.background = '#eee';

    const name = event.dataTransfer.files[0].name.split('.');
    const extension = name[name.length - 1];

    if (event.dataTransfer.files.length > 1) {
      console.log('SOK');
      // TODO notification
    } else if (extension !== 'p4' && extension !== 'txt') {
      console.log('ROSSZ FAJL');
      // TODO notificitaon
    } else {
      this.file.emit(event.dataTransfer.files[0]);
    }
  }
}
