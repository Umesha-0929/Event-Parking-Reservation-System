import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl.replace(/\/$/, '');
  private url(path: string): string { return `${this.baseUrl}/${path.replace(/^\//, '')}`; }

  get<T>(path: string, params?: Record<string, string | number | boolean | undefined>): Observable<T> {
    let httpParams = new HttpParams();
    Object.entries(params ?? {}).forEach(([key, value]) => { if (value !== undefined) httpParams = httpParams.set(key, String(value)); });
    return this.http.get<T>(this.url(path), { params: httpParams });
  }
  post<TResponse, TBody = unknown>(path: string, body: TBody): Observable<TResponse> { return this.http.post<TResponse>(this.url(path), body); }
  put<TResponse, TBody = unknown>(path: string, body: TBody): Observable<TResponse> { return this.http.put<TResponse>(this.url(path), body); }
  patch<TResponse, TBody = unknown>(path: string, body: TBody): Observable<TResponse> { return this.http.patch<TResponse>(this.url(path), body); }
  delete<T>(path: string): Observable<T> { return this.http.delete<T>(this.url(path)); }
}
