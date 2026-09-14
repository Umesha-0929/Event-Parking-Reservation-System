import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';
import { AuthService } from '../../core/services/auth.service';
import { AdminUser, roleLabel } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-admin-users',
  standalone:true,
  imports:[FormsModule,DatePipe,PageStateComponent],
  template:`<div class="nv-page space-y-6">
    <div><p class="font-extrabold text-sm nv-muted">Account Management</p><h1 class="nv-page-title">Users</h1><p class="nv-muted mt-1">Suspend, reactivate or permanently remove Nvent accounts.</p></div>
    <section class="nv-card p-5 md:p-6">
      <div class="flex flex-wrap gap-3"><input class="nv-input max-w-md" [(ngModel)]="query" placeholder="Search name or email" aria-label="Search name or email"><select class="nv-input max-w-56" [(ngModel)]="roleFilter" aria-label="Role filter"><option value="">All roles</option><option value="0">Customer</option><option value="1">Event Organizer</option><option value="2">Venue Owner</option><option value="3">Admin</option></select></div>
      @if(loading()){
        <div class="space-y-3 mt-5" aria-label="Loading users">@for(i of [1,2,3,4];track i){<div class="nv-skeleton h-16"></div>}</div>
      } @else if(loadError()){
        <div class="mt-5"><app-page-state kind="error" title="Users unavailable" message="We couldn't load user accounts right now." actionLabel="Retry" [action]="retry"/></div>
      } @else if(filtered().length){
        <div class="overflow-x-auto mt-5"><table class="w-full text-sm"><caption class="sr-only">Nvent user accounts</caption><thead><tr class="text-left nv-muted"><th class="py-3 pr-4">User</th><th class="py-3 pr-4">Role</th><th class="py-3 pr-4">Status</th><th class="py-3 pr-4">Created</th><th class="py-3">Actions</th></tr></thead><tbody>@for(u of filtered();track u.userId){<tr class="border-t" style="border-color:var(--border)"><td class="py-4 pr-4"><div class="font-black">{{u.firstName}} {{u.lastName}} @if(isSelf(u)){<span class="nv-muted text-xs font-bold">(You)</span>}</div><div class="nv-muted text-xs">{{u.email}}</div></td><td class="py-4 pr-4">{{role(u.role)}}</td><td class="py-4 pr-4"><span class="nv-status" [class.success]="u.status===0" [class.error]="u.status===2">{{status(u.status)}}</span></td><td class="py-4 pr-4">{{u.createdAtUtc|date:'mediumDate'}}</td><td class="py-4"><div class="flex flex-wrap gap-2 min-w-[300px]"><select class="nv-input min-w-36" [ngModel]="u.status" (ngModelChange)="setStatus(u,$event)" [disabled]="isSelf(u)||busyUserId()===u.userId" [attr.aria-label]="'Change status for '+u.firstName"><option [ngValue]="0">Active</option><option [ngValue]="1">Inactive</option><option [ngValue]="2">Suspended</option></select><button type="button" class="nv-btn nv-btn-secondary" style="color:var(--danger);border-color:color-mix(in srgb,var(--danger) 45%,var(--border))" [disabled]="isSelf(u)||busyUserId()===u.userId" (click)="requestDelete(u)">{{busyUserId()===u.userId?'Deleting...':'Delete Permanently'}}</button></div>@if(isSelf(u)){<div class="nv-muted text-xs mt-1">Your current admin account is protected.</div>}</td></tr>}</tbody></table></div>
      } @else {
        <div class="mt-5"><app-page-state title="No matching users" message="No user accounts match the current search and role filter."/></div>
      }
    </section>
    @if(deleteCandidate(); as target){
      <div class="fixed inset-0 z-50 grid place-items-center p-4" style="background:rgba(5,10,30,.64)" role="dialog" aria-modal="true" aria-labelledby="delete-user-title">
        <section class="nv-card p-6 sm:p-7 w-full max-w-lg shadow-2xl">
          <div class="w-12 h-12 rounded-2xl grid place-items-center font-black text-xl" style="background:color-mix(in srgb,var(--danger) 13%,var(--surface));color:var(--danger)">!</div>
          <h2 id="delete-user-title" class="text-2xl font-black mt-4">Delete account permanently?</h2>
          <p class="nv-muted mt-2">You are about to permanently delete <strong>{{target.firstName}} {{target.lastName}}</strong> ({{target.email}}). This action cannot be undone.</p>
          <div class="mt-6 flex flex-col-reverse sm:flex-row sm:justify-end gap-2">
            <button type="button" class="nv-btn nv-btn-secondary" [disabled]="busyUserId()===target.userId" (click)="cancelDelete()">Cancel</button>
            <button type="button" class="nv-btn nv-btn-secondary" style="background:var(--danger);border-color:var(--danger);color:white" [disabled]="busyUserId()===target.userId" (click)="confirmDelete()">{{busyUserId()===target.userId?'Deleting...':'Delete Permanently'}}</button>
          </div>
        </section>
      </div>
    }
    @if(message()){<div class="fixed right-4 bottom-4 nv-card px-4 py-3 max-w-sm" role="status">{{message()}}</div>}
  </div>`
})
export class AdminUsersComponent implements OnInit{
  private api=inject(AdminService);
  private auth=inject(AuthService);
  private route=inject(ActivatedRoute);
  users=signal<AdminUser[]>([]);
  query='';
  roleFilter='';
  message=signal('');
  loading=signal(true);
  loadError=signal(false);
  busyUserId=signal<string|null>(null);
  deleteCandidate=signal<AdminUser|null>(null);
  retry=()=>this.load();
  filtered=computed(()=>{const q=this.query.toLowerCase().trim();return this.users().filter(u=>(!this.roleFilter||String(u.role)===this.roleFilter)&&(!q||`${u.firstName} ${u.lastName} ${u.email}`.toLowerCase().includes(q)));});
  ngOnInit(){this.route.queryParamMap.subscribe(params=>this.query=params.get('q')??'');this.load();}
  load(){this.loading.set(true);this.loadError.set(false);this.api.users({page:1,pageSize:100}).subscribe({next:v=>{this.users.set(v);this.loading.set(false);},error:()=>{this.loading.set(false);this.loadError.set(true);}});}
  role(v:number){return roleLabel(v);}
  status(v:number){return ['Active','Inactive','Suspended'][Number(v)]??String(v);}
  isSelf(u:AdminUser){return this.auth.session()?.userId===u.userId;}
  setStatus(u:AdminUser,status:number){
    if(this.isSelf(u))return;
    this.busyUserId.set(u.userId);
    this.api.setStatus(u.userId,Number(status)).subscribe({next:x=>{this.users.update(xs=>xs.map(y=>y.userId===u.userId?x:y));this.busyUserId.set(null);this.message.set(Number(status)===2?'Account suspended. Sign-in and API access are now blocked.':'Account status updated.');},error:e=>{this.busyUserId.set(null);this.message.set(e?.error?.message||'Account could not be updated.');}});
  }
  requestDelete(u:AdminUser){if(!this.isSelf(u))this.deleteCandidate.set(u);}
  cancelDelete(){if(!this.busyUserId())this.deleteCandidate.set(null);}
  confirmDelete(){
    const u=this.deleteCandidate();
    if(!u||this.isSelf(u))return;
    this.busyUserId.set(u.userId);
    this.api.deleteUser(u.userId).subscribe({next:()=>{this.users.update(xs=>xs.filter(x=>x.userId!==u.userId));this.busyUserId.set(null);this.deleteCandidate.set(null);this.message.set('Account permanently deleted.');},error:e=>{this.busyUserId.set(null);this.message.set(e?.error?.message||'Account could not be deleted.');}});
  }
}
