import { Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from '../services/http-error-handler.service';
import { FileData } from '../models/file';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Authorization': 'auth-token'
  })
};

export class HomeService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient,  httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('HomeService');
    this.baseUrl = baseUrl;
  }

  sendFileContent(fileData: FileData): Observable<FileData> {
    return this.http.post<FileData>(this.baseUrl + 'graph', fileData, httpOptions)
      .pipe(
        catchError(this.handleError('sendFileContent', fileData))
      );
  }
}
