import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService } from '../../core/services/admin.service';
import { AdminDashboardStats } from '../../core/models/api.models';
import { PageStateComponent } from '../../shared/components/page-state.component';

@Component({
  selector:'app-admin-dashboard',
  standalone:true,
  imports:[RouterLink,DecimalPipe,PageStateComponent],
  template:`<div class="nv-page space-y-6">
    <div><p class="font-extrabold text-sm nv-muted">Platform Administration</p><h1 class="nv-page-title">Admin Dashboard</h1><p class="nv-muted mt-1">Platform health, operational totals and areas requiring attention.</p></div>
    @if(loading()){
      <div class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4" aria-label="Loading dashboard totals">@for(i of [1,2,3,4];track i){<div class="nv-card p-5"><div class="nv-skeleton h-4 w-24"></div><div class="nv-skeleton h-9 w-20 mt-3"></div><div class="nv-skeleton h-3 w-32 mt-3"></div></div>}</div>
    } @else if(stats();as s){
      <div class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4"><article class="nv-card p-5"><div class="nv-muted text-sm">Users</div><div class="text-3xl font-black mt-2">{{s.totalUsers}}</div><div class="text-xs nv-muted mt-2">{{s.activeUsers}} active · {{s.suspendedUsers}} suspended</div></article><article class="nv-card p-5"><div class="nv-muted text-sm">Events</div><div class="text-3xl font-black mt-2">{{s.totalEvents}}</div><div class="text-xs nv-muted mt-2">{{s.publishedEvents}} published</div></article><article class="nv-card p-5"><div class="nv-muted text-sm">Bookings</div><div class="text-3xl font-black mt-2">{{s.confirmedBookings}}</div><div class="text-xs nv-muted mt-2">{{s.pendingBookings}} pending</div></article><article class="nv-card p-5"><div class="nv-muted text-sm">Successful revenue</div><div class="text-3xl font-black mt-2">LKR {{s.successfulRevenue|number:'1.0-0'}}</div><div class="text-xs nv-muted mt-2">{{s.successfulPayments}} payments</div></article></div>
    } @else {
      <app-page-state kind="error" title="Dashboard totals unavailable" message="Management pages are still available while the summary service recovers." actionLabel="Retry" [action]="retry"/>
    }
    <div class="grid xl:grid-cols-[1.2fr_.8fr] gap-5"><section class="nv-card p-5 md:p-6"><h2 class="font-black text-xl">Management workspace</h2><div class="grid sm:grid-cols-2 gap-3 mt-5">@for(a of actions;track a.path){<a [routerLink]="a.path" class="nv-card-soft p-4"><div class="font-black">{{a.label}}</div><p class="nv-muted text-sm mt-1">{{a.note}}</p></a>}</div></section><aside class="space-y-4"><section class="nv-card p-5"><h2 class="font-black">Attention</h2>@if(stats();as s){<div class="grid gap-3 mt-4"><a routerLink="/admin/bookings" class="nv-card-soft p-3 flex justify-between"><span>Pending bookings</span><strong>{{s.pendingBookings}}</strong></a><a routerLink="/admin/users" class="nv-card-soft p-3 flex justify-between"><span>Suspended users</span><strong>{{s.suspendedUsers}}</strong></a></div>}@else{<p class="nv-muted text-sm mt-3">Summary counts are temporarily unavailable.</p>}</section><a routerLink="/admin/audit-logs" class="nv-card p-5 block"><div class="font-black">Audit Logs</div><p class="nv-muted text-sm mt-2">Review platform administration and state changes.</p></a></aside></div>
  </div>`
})
export class AdminDashboardComponent implements OnInit{
  private api=inject(AdminService);
  stats=signal<AdminDashboardStats|null>(null);
  loading=signal(true);
  retry=()=>this.load();
  actions=[{label:'Users',path:'/admin/users',note:'Account roles and status.'},{label:'Events',path:'/admin/events',note:'Published platform events.'},{label:'Venues',path:'/admin/venues',note:'Active venue inventory.'},{label:'Payments',path:'/admin/payments',note:'Payment proofs and review.'},{label:'Catalog',path:'/admin/catalog',note:'Categories and venue facilities.'},{label:'Reports',path:'/admin/reports',note:'Platform analytics and revenue.'}];
  ngOnInit(){this.load();}
  load(){this.loading.set(true);this.api.stats().subscribe({next:v=>{this.stats.set(v);this.loading.set(false);},error:()=>{this.stats.set(null);this.loading.set(false);}});}
}
