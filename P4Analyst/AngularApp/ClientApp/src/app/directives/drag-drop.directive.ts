import { Directive, HostBinding, HostListener, Output, EventEmitter } from '@angular/core';

@Directive({
  selector: '[appDrag]'
})

export class DragDropDirective {
  @Output() files: EventEmitter<File[]> = new EventEmitter();

  @HostBinding('style.background') private background = '#eee';

  @HostListener('dragover', ['$event']) public onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.background = '#aaa';
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

    const files = new Array<File>();
    for (let i = 0; i < event.dataTransfer.files.length; ++i) {
      files.push(event.dataTransfer.files.item(i));
    }

    this.files.emit(files);
  }
}
