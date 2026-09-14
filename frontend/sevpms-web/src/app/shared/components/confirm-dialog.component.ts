import { AfterViewInit, Component, ElementRef, EventEmitter, HostListener, Input, OnDestroy, Output, ViewChild } from '@angular/core';

@Component({
  selector:'app-confirm-dialog',
  standalone:true,
  template:`
  <div class="fixed inset-0 z-[110] bg-black/55 p-4 grid place-items-center" (click)="cancel()">
    <section #dialog class="nv-card p-5 sm:p-6 w-full max-w-md" role="alertdialog" aria-modal="true" [attr.aria-labelledby]="titleId" [attr.aria-describedby]="messageId" (click)="$event.stopPropagation()">
      <h2 [id]="titleId" class="text-xl font-black">{{title}}</h2>
      <p [id]="messageId" class="nv-muted mt-2">{{message}}</p>
      <div class="flex flex-col-reverse sm:flex-row sm:justify-end gap-2 mt-6">
        <button #cancelButton type="button" class="nv-btn nv-btn-secondary" (click)="cancel()">{{cancelLabel}}</button>
        <button type="button" class="nv-btn" [class.nv-btn-primary]="tone!=='danger'" [style.background]="tone==='danger'?'var(--danger)':null" [style.color]="tone==='danger'?'white':null" (click)="confirm()">{{confirmLabel}}</button>
      </div>
    </section>
  </div>`
})
export class ConfirmDialogComponent implements AfterViewInit,OnDestroy{
  @Input() title='Confirm action';
  @Input() message='Are you sure you want to continue?';
  @Input() confirmLabel='Confirm';
  @Input() cancelLabel='Cancel';
  @Input() tone:'default'|'danger'='default';
  @Output() confirmed=new EventEmitter<void>();
  @Output() cancelled=new EventEmitter<void>();
  @ViewChild('dialog') dialog?:ElementRef<HTMLElement>;
  @ViewChild('cancelButton') cancelButton?:ElementRef<HTMLButtonElement>;
  readonly titleId=`confirm-title-${Math.random().toString(36).slice(2)}`;
  readonly messageId=`confirm-message-${Math.random().toString(36).slice(2)}`;
  private returnFocus:HTMLElement|null=typeof document!=='undefined'?document.activeElement as HTMLElement:null;
  ngAfterViewInit(){this.cancelButton?.nativeElement.focus();}
  confirm(){this.confirmed.emit();}
  cancel(){this.cancelled.emit();}
  @HostListener('document:keydown',['$event']) onKeydown(event:KeyboardEvent){
    if(event.key==='Escape'){event.preventDefault();this.cancel();return;}
    if(event.key!=='Tab'||!this.dialog)return;
    const nodes=Array.from(this.dialog.nativeElement.querySelectorAll<HTMLElement>('button:not([disabled]),a[href],input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])'));
    if(!nodes.length)return;
    const first=nodes[0],last=nodes[nodes.length-1];
    if(event.shiftKey&&document.activeElement===first){event.preventDefault();last.focus();}
    else if(!event.shiftKey&&document.activeElement===last){event.preventDefault();first.focus();}
  }
  ngOnDestroy(){setTimeout(()=>this.returnFocus?.focus());}
}
