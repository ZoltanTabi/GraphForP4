import { Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { HttpErrorHandler, HandleError } from 'src/app/services/http-error-handler.service';
import { Node } from '../models/graph/node';

export class GraphService {
  private handleError: HandleError;
  private baseUrl: string;

  constructor(private http: HttpClient, httpErrorHandler: HttpErrorHandler, @Inject('BASE_URL') baseUrl: string) {
    this.handleError = httpErrorHandler.createHandleError('HomeService');
    this.baseUrl = baseUrl;
  }

  getGraph(type: string): Observable<Array<Node>> {
    return this.http.get<Array<Node>>(this.baseUrl + 'graph/' + type)
      .pipe(
        catchError(this.handleError<Array<Node>>('getGraph'))
      );
  }
}
