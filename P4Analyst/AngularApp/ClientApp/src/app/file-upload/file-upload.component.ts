import { Component, Output, EventEmitter } from '@angular/core';
import { FileData } from '../models/file';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
  @Output() change: EventEmitter<FileData> = new EventEmitter();

  file: File;
  fileData: FileData;

  constructor(private notificationService: NotificationService) {
    this.fileData = {
      name: '',
      content: ''
    };
    this.file = null;
  }

  filesDropped(files: File[]): void {
    const name = files[0].name.split('.');
    const extension = name[name.length - 1];

    if (files.length > 1) {
      this.notificationService.warning('Csak egy fájlt lehet feltölteni!');
    } else if (extension !== 'p4' && extension !== 'txt') {
      this.notificationService.warning('Megengedett fájlformátumok: ".p4", ".txt"!');
    } else {
      this.file = files[0];
      this.fileRead();
    }
  }

  onUpload(fileList: FileList): void {
    console.log('ABLAK');
    this.file = fileList[0];
    this.fileRead();
  }

  editFile() {

  }

  removeFile() {
    console.log('DELETE');
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
    console.log('KULDES');
    this.change.emit(this.fileData);
  }

  getName(): string {
    return this.fileData.name ? this.fileData.name : '';
  }
}
