import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { error } from '@angular/compiler/src/util';
import { HomeService } from './home.service';
import { FileData } from '../models/file';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  providers: [HomeService],
  styleUrls: ['./home.component.scss']
})

export class HomeComponent implements OnInit {
  fileData: FileData;
  file: File;
  fileName: string;
  fileContent: string;

  constructor(private homeService: HomeService, private router: Router) {
    this.fileData = {
      name: '',
      content: ''
    };
  }

  ngOnInit(): void {
    this.file = null;
  }


  trigger() {
    const element = document.getElementById('upload_file') as HTMLInputElement;
    element.click();
  }

  onChange(fileList: FileList): void {
    this.file = fileList[0];
    this.fileData.name = fileList[0].name;

    const fileReader: FileReader = new FileReader();
    const self = this;
    fileReader.onloadend = () => {
      self.fileData.content = fileReader.result.toString();
    };
    fileReader.readAsText(this.file);
    this.changeFile(this.fileData);
  }

  removeFile() {
    this.file = null;
    this.fileData.name = '';
    this.fileData.content = '';
    this.changeFile(this.fileData);
  }

  onNext() {
    const promise = this.homeService
      .sendFileContent(this.fileData)// .toPromise();
      .subscribe(result => {
        this.fileData.name = result.name;
        this.print();
      });
    // promise.then((data) => {
   //   this.fileData.name = data.name;
    //  console.log("Promise resolved with: " + this.fileData.name);
   // }).catch((error) => {
    //  console.log("Promise rejected with " + JSON.stringify(error));
    // });
    // console.log(this.fileData.name);
  }

  private changeFile(fileData: FileData) {
    this.fileName = fileData.name;
    this.fileContent = fileData.content;
  }

  private print() {
    console.log(this.fileData.name);
  }
}
