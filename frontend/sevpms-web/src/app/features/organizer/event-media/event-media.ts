import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { LocalMediaService } from '../../../core/services/local-media.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
@Component({selector:'app-organizer-event-media',imports:[RouterLink],templateUrl:'./event-media.html',styleUrl:'./event-media.scss'})
export class OrganizerEventMediaComponent implements OnInit{
 private readonly route=inject(ActivatedRoute);private readonly media=inject(LocalMediaService);readonly eventId=this.route.snapshot.paramMap.get('eventId')??'';readonly cover=signal('');readonly busy=signal(false);readonly error=signal('');readonly success=signal('');readonly dragging=signal(false);
 ngOnInit():void{if(!this.eventId)return;this.media.eventCover(this.eventId).pipe(catchError(()=>of(null))).subscribe(x=>this.cover.set(x?.url??''));}
 choose(input:HTMLInputElement):void{input.click();} inputChanged(e:Event):void{const input=e.target as HTMLInputElement;const file=input.files?.[0];if(file)this.upload(file);input.value='';}
 dragOver(e:DragEvent):void{e.preventDefault();this.dragging.set(true);} leave(e:DragEvent):void{e.preventDefault();this.dragging.set(false);} drop(e:DragEvent):void{e.preventDefault();this.dragging.set(false);const file=e.dataTransfer?.files?.[0];if(file)this.upload(file);}
 paste(e:ClipboardEvent):void{const item=[...(e.clipboardData?.items??[])].find(x=>x.kind==='file');const file=item?.getAsFile();if(file){e.preventDefault();this.upload(file);}}
 private upload(file:File):void{if(!file.type.startsWith('image/')){this.error.set('Event cover must be an image.');return;}this.busy.set(true);this.error.set('');this.media.uploadEventCover(this.eventId,file).subscribe({next:x=>{this.cover.set(x.url);this.busy.set(false);this.success.set('Event cover updated. Customers will see this same image.');},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'Event cover could not be uploaded.'));}});}
}
