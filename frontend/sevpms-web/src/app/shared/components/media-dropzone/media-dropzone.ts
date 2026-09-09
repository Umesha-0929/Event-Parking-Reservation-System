import { Component, EventEmitter, HostListener, Input, Output, inject, signal } from '@angular/core';
import { LocalMediaService, UploadedLocalMedia } from '../../../core/services/local-media.service';
import { httpErrorMessage } from '../../../core/utils/http-error';

@Component({
  selector: 'app-media-dropzone',
  standalone: true,
  templateUrl: './media-dropzone.html',
  styleUrl: './media-dropzone.scss',
})
export class MediaDropzoneComponent {
  private readonly media = inject(LocalMediaService);
  @Input() category = 'general';
  @Input() imageOnly = false;
  @Input() compact = false;
  @Output() uploaded = new EventEmitter<UploadedLocalMedia>();
  readonly busy = signal(false); readonly error = signal(''); readonly dragging = signal(false);

  choose(input: HTMLInputElement): void { input.click(); }
  onInput(event: Event): void { const input = event.target as HTMLInputElement; const file = input.files?.[0]; if (file) this.process(file); input.value=''; }
  onDragOver(event: DragEvent): void { event.preventDefault(); this.dragging.set(true); }
  onDragLeave(event: DragEvent): void { event.preventDefault(); this.dragging.set(false); }
  onDrop(event: DragEvent): void { event.preventDefault(); this.dragging.set(false); const file=event.dataTransfer?.files?.[0]; if(file)this.process(file); }
  @HostListener('paste', ['$event']) onPaste(event: ClipboardEvent): void {
    const items=[...(event.clipboardData?.items??[])]; const item=items.find(x=>x.kind==='file'); const file=item?.getAsFile(); if(file){ event.preventDefault(); this.process(file); }
  }
  private process(file: File): void {
    this.error.set('');
    if(this.imageOnly && !file.type.startsWith('image/')){this.error.set('Choose an image file.');return;}
    if(!file.type.startsWith('image/')&&!file.type.startsWith('video/')){this.error.set('Choose an image or video file.');return;}
    this.busy.set(true);
    this.media.upload(file,this.category).subscribe({next:x=>{this.busy.set(false);this.uploaded.emit(x);},error:e=>{this.busy.set(false);this.error.set(httpErrorMessage(e,'The media file could not be uploaded.'));}});
  }
}
