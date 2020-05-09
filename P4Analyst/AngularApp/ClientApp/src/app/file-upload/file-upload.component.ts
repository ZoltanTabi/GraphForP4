import { Component, Output, EventEmitter } from '@angular/core';
import { FileData } from '../models/file';
import { NotificationService } from '../services/notification.service';
import { MatDialog } from '@angular/material';
import { EditDialogComponent } from './edit-dialog/edit-dialog.component';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
  @Output() change: EventEmitter<FileData> = new EventEmitter();

  file: File;
  fileData: FileData;

  constructor(private notificationService: NotificationService, public dialog: MatDialog) {
    this.fileData = {
      name: '',
      content: ''
    };
    this.file = null;
  }

  filesDropped(files: File[]): void {
    if (files.length > 1) {
      this.notificationService.warning('Csak egy fájlt lehet feltölteni!');
    } else {
      this.fileValidation(files[0]);
    }
  }

  onUpload(fileList: FileList): void {
    const file = fileList[0];
    this.fileValidation(file);
    this.fileRead();
  }

  fileValidation(file: File): void {
    const name = file.name.split('.');
    const extension = name[name.length - 1];

    if (extension !== 'p4' && extension !== 'txt') {
      this.notificationService.warning('Megengedett fájlformátumok: ".p4", ".txt"!');
    } else {
      this.file = file;
      this.fileRead();
    }
  }

  editFile() {
    const dialogRef = this.dialog.open(EditDialogComponent, {
      height: '30vw',
      width: '55vw',
      panelClass: 'my-dark-theme',
      data: { content: this.fileData.content }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.fileData.content = result;
        this.onChange();
        this.notificationService.success('Sikeres szerkesztés!');
      }
    });
  }

  removeFile() {
    this.fileData.name = '';
    this.fileData.content = '';
    this.file = null;
    this.onChange();
  }

  fileRead(): void {
    this.fileData.name = this.file.name;

    const fileReader: FileReader = new FileReader();
    const self = this;
    fileReader.onloadend = () => {
      self.fileData.content = fileReader.result.toString();
      self.onChange();
    };
    fileReader.readAsText(this.file);
  }

  onChange() {
    this.change.emit(this.fileData);
  }

  getName(): string {
    return this.fileData.name ? this.fileData.name : '';
  }
}
