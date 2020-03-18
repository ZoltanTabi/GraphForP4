import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  fileName: string;
  file: File;

  constructor(public http: HttpClient, public router: Router) { }

  ngOnInit(): void {
    this.file = null;
  }


  trigger() {
    let element = document.getElementById('upload_file') as HTMLInputElement;
    element.click();
  }

  onChange(file) {
    this.file = file.files[0];
    this.fileName = file.files[0].name;
  }

  removeFile() {
    this.file = null;
    this.fileName = '';
  }

  next(){
    this.http.post<File>('http://localhost:6687/' + 'api/fileupload', this.file).subscribe(
      (response) => {this.router.navigate(['graph']);
    });
  }
}
