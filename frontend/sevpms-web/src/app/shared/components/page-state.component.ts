import { Component, Input } from '@angular/core';
@Component({selector:'app-page-state',standalone:true,template:`
<div class="nv-card p-8 text-center min-h-56 flex flex-col items-center justify-center gap-3" [attr.role]="kind==='error'?'alert':'status'">
  <div class="nv-page-state-art" [style.backgroundImage]="artBackground"><span>{{icon}}</span></div>
  <h3 class="font-black text-lg tracking-[-.01em]">{{title}}</h3><p class="nv-muted max-w-lg">{{message}}</p>
  @if(actionLabel){<button type="button" class="nv-btn nv-btn-secondary mt-1" (click)="action?.()">{{actionLabel}}</button>}
</div>`})
export class PageStateComponent{
  @Input() kind:'empty'|'error'|'offline'|'success'='empty'; @Input() title='Nothing here yet'; @Input() message=''; @Input() actionLabel=''; @Input() action?:()=>void;
  get icon(){return this.kind==='error'?'!':this.kind==='offline'?'○':this.kind==='success'?'✓':'◇';}
  get artBackground(){const file=this.kind==='error'?'error':this.kind==='offline'?'offline':'no-results';return `linear-gradient(145deg,color-mix(in srgb,var(--surface) 78%,transparent),color-mix(in srgb,var(--focus) 18%,var(--surface))),url('assets/nvent/3d/empty-states/${file}.webp')`;}
}
