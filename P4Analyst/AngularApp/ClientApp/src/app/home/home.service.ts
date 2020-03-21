import { Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';


import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { HttpErrorHandler, HandleError } from 'src/app/services/http-error-handler.service';
import { FileData } from '../models/file';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Authorization': 'my-auth-token'
  })
};

export class HomeService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(
    private http: HttpClient,
    httpErrorHandler: HttpErrorHandler,
    @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('HomeService');
    this.baseUrl = baseUrl;
  }

  sendFileContent(fileData: FileData): Observable<FileData> {
    return this.http.post<FileData>(this.baseUrl + 'fileupload', fileData, httpOptions)
      .pipe(
        catchError(this.handleError('sendFileContent', fileData))
      );
  }
}
