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
  code: string;

  constructor(private homeService: HomeService, private router: Router) {
    this.fileData = {
      name: '',
      content: ''
    };
    this.code = '';
  }

  ngOnInit(): void { }

  onChange(fileData: FileData) {
    this.fileData = fileData;
  }

  getNextDisabled(): boolean {
    return !this.isCode() && !this.isFile();
  }

  isCode(): Boolean {
    return this.code !== '';
  }

  isFile(): Boolean {
    return this.fileData.name !== '';
  }

  onNext() {
    if (this.isCode()) {
      this.fileData.content = this.code;
    }
    this.homeService
      .sendFileContent(this.fileData)
      .subscribe(() => {
        this.navigate();
      });
  }

  navigate() {
    this.router.navigateByUrl('/graphview');
  }
}
