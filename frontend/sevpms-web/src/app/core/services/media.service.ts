import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { MediaUploadDto, MediaUrlDto } from '../models/api.models';

@Injectable({providedIn:'root'})
export class MediaService{
  private http=inject(HttpClient);
  upload(file:File,category='events'){
    const fd=new FormData();fd.append('file',file);
    return this.http.post<MediaUploadDto>(`${environment.apiBaseUrl}/media/uploads?category=${encodeURIComponent(category)}`,fd,{withCredentials:true});
  }
  eventCover(eventId:string,file:File){const fd=new FormData();fd.append('file',file);return this.http.post<MediaUrlDto>(`${environment.apiBaseUrl}/events/${eventId}/cover`,fd,{withCredentials:true});}
  paymentQr(file:File){const fd=new FormData();fd.append('file',file);return this.http.post<MediaUrlDto>(`${environment.apiBaseUrl}/payment-qr/mine`,fd,{withCredentials:true});}
}
