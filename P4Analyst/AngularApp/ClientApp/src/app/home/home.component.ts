import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HomeService } from './home.service';
import { FileData } from '../models/file';
import { SessionStorageService } from 'ngx-store';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  providers: [HomeService],
  styleUrls: ['./home.component.scss']
})

export class HomeComponent implements OnInit {
  fileData: FileData;
  code: string;

  constructor(private homeService: HomeService, private router: Router, private sessionStorageService: SessionStorageService) {
    this.fileData = {
      id: 0,
      name: '',
      content: '',
      createDate: new Date()
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
        this.sessionStorageService.clear('prefix');
        this.navigate();
      });
  }

  navigate() {
    this.router.navigateByUrl('/graphview');
  }
}
