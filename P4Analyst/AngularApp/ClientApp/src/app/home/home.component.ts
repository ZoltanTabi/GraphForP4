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

  constructor(private homeService: HomeService, private router: Router) {
    this.fileData = {
      name: '',
      content: ''
    };
  }

  ngOnInit(): void { }

  onUpload(fileData: FileData) {
    console.log('Megérkezett');
    this.fileData = fileData;
    this.fileData.name = this.fileData.name ? this.fileData.name : '';
  }

  onNext() {
    this.homeService
      .sendFileContent(this.fileData)
      .subscribe(result => {
        this.fileData.name = result.name;
      }, () => {
        console.log(error);
      }, () => {
        this.navigate();
      });
  }

  navigate() {
    this.router.navigateByUrl('/graphview');
  }
}
