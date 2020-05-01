import { Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from 'src/app/services/http-error-handler.service';
import { Struct } from '../models/variables/struct';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Authorization': 'my-auth-token'
  })
};

export class AnalyzeService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('AnalyzeService');
    this.baseUrl = baseUrl;
  }

  getVariables(): Observable<Array<Struct>> {
    return this.http.get<Array<Struct>>(this.baseUrl + 'analyzer')
      .pipe(
        catchError(this.handleError<Array<Struct>>('getVariables'))
      );
  }

  putStructs(structs: Array<[Struct[], Struct[]]>) {
    return this.http.put<Array<Struct>>(this.baseUrl + 'analyzer', structs, httpOptions)
      .pipe(
        catchError(this.handleError<Array<Struct>>('putStructs'))
      );
  }
}
