import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { throwError, Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

export abstract class BaseService {
  protected constructor(protected http: HttpClient) {}

  protected get<T>(url: string): Observable<T> {
    return this.http.get<T>(url).pipe(catchError(this.handleError));
  }

  protected post<T>(url: string, body: any): Observable<T> {
    return this.http.post<T>(url, body).pipe(catchError(this.handleError));
  }

  protected put<T>(url: string, body: any): Observable<T> {
    return this.http.put<T>(url, body).pipe(catchError(this.handleError));
  }

  protected delete<T>(url: string): Observable<T> {
    return this.http.delete<T>(url).pipe(catchError(this.handleError));
  }

  private handleError(error: HttpErrorResponse) {
    console.error('[API ERROR]', error);
    const message =
      error.error?.message ||
      error.statusText ||
      'Unexpected API error occurred.';
    return throwError(() => new Error(message));
  }
}