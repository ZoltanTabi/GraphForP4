import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { HttpErrorHandler, HandleError } from './http-error-handler.service';
import { catchError } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { FileData } from '../models/file';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Authorization': 'my-auth-token'
  })
};

@Injectable()
export class FileService {

  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient,  httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('FileService');
    this.baseUrl = baseUrl;
  }

  getFile(id: number): Observable<FileData> {
    return this.http.get<FileData>(this.baseUrl + 'file/' + id)
      .pipe(
        catchError(this.handleError<FileData>('getFile'))
      );
  }

  getFiles(): Observable<Array<FileData>> {
    return this.http.get<Array<FileData>>(this.baseUrl + 'file')
      .pipe(
        catchError(this.handleError<Array<FileData>>('getFiles'))
      );
  }

  uploadFile(fileData: FileData): Observable<FileData> {
    return this.http.post<FileData>(this.baseUrl + 'file', fileData, httpOptions)
      .pipe(
        catchError(this.handleError('UploadFile', fileData))
      );
  }
}
