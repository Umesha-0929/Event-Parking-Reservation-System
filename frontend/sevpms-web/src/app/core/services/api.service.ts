import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl.replace(/\/$/, '');
  get<T>(path:string, params?:Record<string, unknown>) { return this.http.get<T>(this.url(path), { params: this.params(params), withCredentials:true }); }
  post<T>(path:string, body:unknown = {}) { return this.http.post<T>(this.url(path), body, { withCredentials:true }); }
  put<T>(path:string, body:unknown = {}) { return this.http.put<T>(this.url(path), body, { withCredentials:true }); }
  patch<T>(path:string, body:unknown = {}) { return this.http.patch<T>(this.url(path), body, { withCredentials:true }); }
  delete<T>(path:string, params?:Record<string, unknown>) { return this.http.delete<T>(this.url(path), { params:this.params(params), withCredentials:true }); }
  blob(path:string, params?:Record<string, unknown>) { return this.http.get(this.url(path), { params:this.params(params), responseType:'blob', withCredentials:true }); }
  private url(path:string) { return `${this.base}/${path.replace(/^\//,'')}`; }
  private params(value?:Record<string, unknown>) {
    let p = new HttpParams();
    if (!value) return p;
    for (const [key,raw] of Object.entries(value)) if (raw !== undefined && raw !== null && raw !== '') p = p.set(key, String(raw));
    return p;
  }
}
