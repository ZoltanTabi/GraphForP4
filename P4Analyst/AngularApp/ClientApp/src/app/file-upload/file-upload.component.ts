import { Component, Output, EventEmitter } from '@angular/core';
import { FileData } from '../models/file';

@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrls: ['./file-upload.component.scss']
})
export class FileUploadComponent {
  @Output() upload: EventEmitter<FileData> = new EventEmitter();

  uploaded = true;
  file: File;
  fileData: FileData;

  constructor() {
    this.fileData = {
      name: '',
      content: ''
    };
    this.file = null;
  }

  fileDropped(file: File): void {
    console.log('DROP');
    this.file = file;
    this.newUpload();
  }

  onChange(fileList: FileList): void {
    console.log('ABLAK');
    this.file = fileList[0];
    this.newUpload();
  }

  removeFile() {
    console.log('DELETE');
    this.fileData.name = '';
    this.fileData.content = '';
    this.file = null;
    this.newUpload();
  }


  newUpload(): void {
    this.uploaded = false;
    this.fileData.name = this.file.name;

    const fileReader: FileReader = new FileReader();
    const self = this;
    fileReader.onloadend = () => {
      self.fileData.content = fileReader.result.toString();
    };
    fileReader.readAsText(this.file);
    console.log('KULDES');
    this.upload.emit(this.fileData);
  }

  getName(): string {
    return this.fileData.name ? this.fileData.name : '';
  }
}
