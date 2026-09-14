import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../core/services/admin.service';
import { AuditLogDto } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-admin-audit-logs',
  standalone:true,
  imports:[FormsModule,DatePipe,PageStateComponent],
  template:`
  <div class="nv-page space-y-6">
    <div><p class="font-extrabold text-sm nv-muted">Security & Traceability</p><h1 class="nv-page-title">Audit Logs</h1><p class="nv-muted mt-1">Read-only record of important platform and administrative state changes.</p></div>
    <section class="nv-card p-5 md:p-6">
      <form class="grid md:grid-cols-2 xl:grid-cols-5 gap-3" (ngSubmit)="load()">
        <div><label class="nv-label">Entity type</label><input class="nv-input" name="entity" [(ngModel)]="entityType" placeholder="Event, Venue, User" aria-label="Event, Venue, User"></div>
        <div><label class="nv-label">Actor user ID</label><input class="nv-input" name="actor" [(ngModel)]="actorUserId" placeholder="Optional UUID" aria-label="Optional UUID"></div>
        <div><label class="nv-label">From</label><input class="nv-input" type="datetime-local" name="from" [(ngModel)]="fromLocal" aria-label="From date"></div>
        <div><label class="nv-label">To</label><input class="nv-input" type="datetime-local" name="to" [(ngModel)]="toLocal" aria-label="To date"></div>
        <div class="flex items-end"><button type="submit" class="nv-btn nv-btn-primary w-full" [disabled]="loading()">{{loading()?'Loading...':'Apply Filters'}}</button></div>
      </form>
      @if(loading() && !logs().length){
        <div class="space-y-3 mt-5" aria-label="Loading audit logs">@for(i of [1,2,3,4];track i){<div class="nv-skeleton h-16"></div>}</div>
      } @else if(loadError()){
        <div class="mt-5"><app-page-state kind="error" title="Audit logs unavailable" message="We couldn't load audit records right now." actionLabel="Retry" [action]="retry"/></div>
      } @else if(logs().length){
        <div class="overflow-x-auto mt-5"><table class="w-full text-sm"><caption class="sr-only">Admin audit log records</caption><thead><tr class="text-left nv-muted"><th class="py-3 pr-4">Time</th><th class="py-3 pr-4">Action</th><th class="py-3 pr-4">Entity</th><th class="py-3 pr-4">Actor</th><th class="py-3">Details</th></tr></thead><tbody>@for(log of logs();track log.auditLogId){<tr class="border-t align-top" style="border-color:var(--border)"><td class="py-4 pr-4 whitespace-nowrap">{{log.createdAtUtc|date:'medium'}}</td><td class="py-4 pr-4 font-extrabold">{{log.action}}</td><td class="py-4 pr-4"><div>{{log.entityType}}</div><div class="text-xs nv-muted">{{short(log.entityId)}}</div></td><td class="py-4 pr-4"><div>{{short(log.actorUserId)}}</div><div class="text-xs nv-muted">{{log.ipAddress||''}}</div></td><td class="py-4 min-w-72"><details><summary class="cursor-pointer font-bold">View change</summary><div class="grid gap-2 mt-3"><div class="nv-card-soft p-3"><div class="text-xs font-extrabold nv-muted">Before</div><pre class="text-xs whitespace-pre-wrap break-words mt-1">{{log.beforeSummary||'—'}}</pre></div><div class="nv-card-soft p-3"><div class="text-xs font-extrabold nv-muted">After</div><pre class="text-xs whitespace-pre-wrap break-words mt-1">{{log.afterSummary||'—'}}</pre></div></div></details></td></tr>}</tbody></table></div>
      } @else {
        <div class="mt-5"><app-page-state title="No audit records" message="No audit records match the current filters."/></div>
      }
    </section>
  </div>`
})
export class AdminAuditLogsComponent implements OnInit{
  private api=inject(AdminService);
  logs=signal<AuditLogDto[]>([]);
  loading=signal(false);
  loadError=signal(false);
  entityType=''; actorUserId=''; fromLocal=''; toLocal='';
  retry=()=>this.load();
  ngOnInit(){this.load();}
  load(){this.loading.set(true);this.loadError.set(false);const params:Record<string,unknown>={take:200};if(this.entityType.trim())params['entityType']=this.entityType.trim();if(this.actorUserId.trim())params['actorUserId']=this.actorUserId.trim();if(this.fromLocal)params['fromUtc']=new Date(this.fromLocal).toISOString();if(this.toLocal)params['toUtc']=new Date(this.toLocal).toISOString();this.api.audits(params).subscribe({next:v=>{this.logs.set(v);this.loading.set(false);},error:()=>{this.loading.set(false);this.loadError.set(true);}});}
  short(value:unknown){const s=String(value??'');return s.length>12?`${s.slice(0,8)}…${s.slice(-4)}`:(s||'—');}
}
