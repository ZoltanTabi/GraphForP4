import { Inject } from '@angular/core';

import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';

import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { HttpErrorHandler, HandleError } from 'src/app/services/http-error-handler.service';
import { Node } from '../models/node';

export class GraphService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(
    private http: HttpClient,
    httpErrorHandler: HttpErrorHandler,
    @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('HomeService');
    this.baseUrl = baseUrl;
  }

  getGraph(type: string): Observable<Array<Node>> {
    const options = type ?
     { params: new HttpParams().set('type', type) } : {};

    return this.http.get<Array<Node>>(this.baseUrl + 'graph/' + type)
      .pipe(
        catchError(this.handleError<Array<Node>>('sendFileContent', []))
      );
  }
}
