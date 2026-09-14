import { Component, ElementRef, HostListener, Input, ViewChild, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { ThemeService } from '../../core/services/theme.service';
import { NotificationsService } from '../../core/services/notifications.service';
import { LocalWeatherService } from '../../core/services/local-weather.service';
import { roleLabel } from '../../core/models/api.models';

interface MenuItem{label:string;path:string;icon:string;}
const customer:MenuItem[]=[{label:'Home',path:'/app/home',icon:'⌂'},{label:'Events',path:'/app/events',icon:'◫'},{label:'Venues',path:'/app/venues',icon:'▣'},{label:'Parking',path:'/app/parking',icon:'P'},{label:'Food & Drinks',path:'/app/food',icon:'◉'},{label:'Places',path:'/app/places',icon:'⌖'},{label:'My Bookings',path:'/app/bookings',icon:'▤'},{label:'Notifications',path:'/app/notifications',icon:'●'},{label:'Profile',path:'/app/profile',icon:'○'}];
const organizer:MenuItem[]=[{label:'Dashboard',path:'/organizer',icon:'⌂'},{label:'My Events',path:'/organizer/events',icon:'◫'},{label:'Host Event',path:'/organizer/host-event',icon:'＋'},{label:'Stage & Seating',path:'/organizer/seating',icon:'▦'},{label:'Bookings',path:'/organizer/bookings',icon:'▤'},{label:'Tickets & Check-In',path:'/organizer/check-in',icon:'⌗'},{label:'Orders',path:'/organizer/orders',icon:'◉'},{label:'Analytics',path:'/organizer/analytics',icon:'⌁'},{label:'Notifications',path:'/organizer/notifications',icon:'●'},{label:'Profile',path:'/organizer/profile',icon:'○'}];
const venue:MenuItem[]=[{label:'Dashboard',path:'/venue-owner',icon:'⌂'},{label:'My Venues',path:'/venue-owner/venues',icon:'▣'},{label:'Add Venue',path:'/venue-owner/add-venue',icon:'＋'},{label:'Rentals',path:'/venue-owner/rentals',icon:'▤'},{label:'Parking',path:'/venue-owner/parking',icon:'P'},{label:'Payments',path:'/venue-owner/payments',icon:'¤'},{label:'Reports',path:'/venue-owner/reports',icon:'⌁'},{label:'Notifications',path:'/venue-owner/notifications',icon:'●'},{label:'Profile',path:'/venue-owner/profile',icon:'○'}];
const admin:MenuItem[]=[{label:'Dashboard',path:'/admin',icon:'⌂'},{label:'Users',path:'/admin/users',icon:'◎'},{label:'Events',path:'/admin/events',icon:'◫'},{label:'Venues',path:'/admin/venues',icon:'▣'},{label:'Bookings',path:'/admin/bookings',icon:'▤'},{label:'Parking',path:'/admin/parking',icon:'P'},{label:'Food Orders',path:'/admin/food',icon:'◉'},{label:'Places',path:'/admin/places',icon:'⌖'},{label:'Payments',path:'/admin/payments',icon:'¤'},{label:'Catalog',path:'/admin/catalog',icon:'▦'},{label:'Notifications',path:'/admin/notifications',icon:'●'},{label:'Audit Logs',path:'/admin/audit-logs',icon:'≡'},{label:'Reports',path:'/admin/reports',icon:'⌁'},{label:'Profile',path:'/admin/profile',icon:'○'}];

@Component({
  selector:'app-sidebar',
  standalone:true,
  imports:[RouterLink,RouterLinkActive,FormsModule],
  template:`
<!-- desktop sidebar -->
<aside class="nv-sidebar-shell hidden lg:flex fixed inset-y-0 left-0 w-[258px] z-40 p-4 flex-col border-r">
  <a [routerLink]="home" class="flex items-center gap-3 px-2 py-2 mb-3"><img src="assets/brand/nvent-mark.svg" class="w-10 h-10" alt=""><span class="text-xl font-black tracking-tight">Nvent</span></a>
  <form class="mb-3" (submit)="search();$event.preventDefault()"><label class="sr-only" for="sidebar-search">Search events</label><input id="sidebar-search" class="nv-input text-sm" [(ngModel)]="query" name="q" [placeholder]="searchPlaceholder" [attr.aria-label]="searchPlaceholder"></form>
  <nav class="flex-1 min-h-0 overflow-auto pr-1 nv-sidebar-nav" aria-label="Primary navigation">@for(item of menu;track item.path){<a [routerLink]="item.path" routerLinkActive="active-nav" ariaCurrentWhenActive="page" [routerLinkActiveOptions]="{exact:item.path===home}" class="sidebar-link"><span class="sidebar-icon">{{item.icon}}</span><span>{{item.label}}</span>@if(item.label==='Notifications'&&notices.unread()>0){<span class="ml-auto text-xs rounded-full px-2 py-0.5 text-white" style="background:var(--danger)">{{notices.unread()}}</span>}</a>}</nav>

  <div class="nv-sidebar-utility mt-3">
    <div class="nv-weather-card" aria-label="Local weather">
      <div class="nv-weather-shade"></div>
      <div class="relative z-[1] h-full flex flex-col justify-between">
        <div class="flex items-start justify-between gap-3">
          <div><div class="font-black text-sm">{{day}}</div><div class="text-xs text-white/80 mt-0.5">{{date}}</div></div>
          <div class="text-right"><div class="text-[11px] font-extrabold uppercase tracking-[.08em] text-white/75">Your area</div><div class="font-black text-sm mt-0.5">{{weatherCondition}}</div></div>
        </div>
        <div>
          <div class="flex items-end gap-2"><span class="text-2xl leading-none" aria-hidden="true">{{weatherIcon}}</span><strong class="text-2xl leading-none">{{temperature}}</strong></div>
          <div class="text-[10px] text-white/80 mt-2 max-w-[150px]">Good days lead to great memories.</div>
        </div>
      </div>
    </div>
    <button type="button" class="nv-theme-toggle mt-2" (click)="theme.toggle()" [attr.aria-label]="theme.dark()?'Switch to light theme':'Switch to dark theme'">
      <span class="nv-theme-toggle-icon" aria-hidden="true">{{theme.dark()?'☀':'☾'}}</span>
      <span class="font-extrabold">{{theme.dark()?'Light theme':'Dark theme'}}</span>
      <span class="ml-auto opacity-60">↔</span>
    </button>
    <div class="mt-2 pt-2 border-t flex items-center gap-3 px-1" style="border-color:var(--border)">
      <a [routerLink]="profilePath" class="w-9 h-9 rounded-full grid place-items-center font-extrabold text-white shrink-0" style="background:linear-gradient(135deg,var(--primary),var(--accent))" aria-label="Open profile">{{initials}}</a>
      <a [routerLink]="profilePath" class="min-w-0 flex-1"><div class="font-extrabold truncate text-sm">{{auth.displayName()}}</div><div class="text-xs nv-muted truncate">{{role}}</div></a>
      @if(auth.isAuthenticated()){<button type="button" class="text-xs nv-muted whitespace-nowrap" (click)="logout()">Sign out</button>}@else{<a routerLink="/auth/sign-in" class="text-xs font-bold">Sign in</a>}
    </div>
  </div>
</aside>

<!-- mobile header / drawer -->
<header class="nv-mobile-header lg:hidden fixed top-0 inset-x-0 z-50 h-16 flex items-center justify-between px-4 border-b"><button type="button" class="w-11 h-11 rounded-xl" (click)="openDrawer()" aria-label="Open navigation" aria-controls="mobile-app-navigation" [attr.aria-expanded]="drawer()">☰</button><a [routerLink]="home" class="flex items-center gap-2 font-black"><img src="assets/brand/nvent-mark.svg" class="w-8 h-8" alt="">Nvent</a><a [routerLink]="notificationsPath" class="relative w-11 h-11 rounded-xl grid place-items-center" aria-label="Notifications">●@if(notices.unread()>0){<span class="absolute top-1 right-1 w-5 h-5 text-[10px] rounded-full bg-red-500 text-white grid place-items-center">{{notices.unread()}}</span>}</a></header>
@if(drawer()){
  <div class="lg:hidden fixed inset-0 z-[60] bg-black/40" (click)="closeDrawer()"></div>
  <aside #mobileDrawer id="mobile-app-navigation" class="lg:hidden fixed inset-y-0 left-0 z-[70] w-[86vw] max-w-[340px] p-4 overflow-auto nv-mobile-drawer" role="dialog" aria-modal="true" aria-label="Application navigation">
    <div class="flex items-center justify-between mb-4"><div class="flex items-center gap-2 font-black text-lg"><img src="assets/brand/nvent-mark.svg" class="w-9 h-9" alt="">Nvent</div><button type="button" class="w-11 h-11" (click)="closeDrawer()" aria-label="Close navigation">×</button></div>
    <form (submit)="search();closeDrawer();$event.preventDefault()" class="mb-4"><input class="nv-input" [(ngModel)]="query" name="mq" [placeholder]="searchPlaceholder" [attr.aria-label]="searchPlaceholder"></form>
    <nav class="space-y-1">@for(item of menu;track item.path){<a [routerLink]="item.path" (click)="closeDrawer()" routerLinkActive="active-nav" ariaCurrentWhenActive="page" class="sidebar-link"><span class="sidebar-icon">{{item.icon}}</span>{{item.label}}</a>}</nav>
    <div class="nv-sidebar-utility mt-5">
      <div class="nv-weather-card nv-weather-card-mobile"><div class="nv-weather-shade"></div><div class="relative z-[1] h-full flex items-end justify-between gap-3"><div><div class="font-black">{{day}} · {{date}}</div><div class="text-xs text-white/80 mt-1">{{weatherCondition}}</div></div><div class="flex items-center gap-2"><span class="text-xl">{{weatherIcon}}</span><strong class="text-xl">{{temperature}}</strong></div></div></div>
      <button type="button" class="nv-theme-toggle mt-2" (click)="theme.toggle()"><span class="nv-theme-toggle-icon">{{theme.dark()?'☀':'☾'}}</span><span class="font-extrabold">{{theme.dark()?'Light theme':'Dark theme'}}</span></button>
      @if(auth.isAuthenticated()){<button type="button" class="nv-btn nv-btn-secondary w-full mt-2" (click)="logout()">Sign out</button>}
    </div>
  </aside>
}
@if(workspace==='customer'){<nav class="nv-mobile-bottom lg:hidden fixed bottom-0 inset-x-0 z-50 h-[72px] px-2 pb-[env(safe-area-inset-bottom)] border-t flex items-center justify-around" aria-label="Mobile navigation">@for(item of bottomMenu;track item.path){<a [routerLink]="item.path" routerLinkActive="text-[var(--primary)]" ariaCurrentWhenActive="page" class="flex flex-col gap-1 items-center justify-center min-w-14 text-[11px] font-bold"><span class="text-lg">{{item.icon}}</span>{{item.label}}</a>}</nav>}
`,
  styles:[`
    .nv-sidebar-shell,.nv-mobile-header,.nv-mobile-drawer,.nv-mobile-bottom{background:color-mix(in srgb,var(--surface) 94%,transparent);border-color:var(--border);backdrop-filter:blur(20px) saturate(135%)}
    .sidebar-link{display:flex;align-items:center;gap:.8rem;min-height:42px;padding:.58rem .72rem;border-radius:14px;color:var(--text-2);font-weight:720;font-size:.88rem;margin:.06rem 0}.sidebar-link:hover{background:var(--surface-2);color:var(--text)}.active-nav{background:color-mix(in srgb,var(--primary) 16%,var(--surface))!important;color:var(--primary-strong)!important}.sidebar-icon{width:1.4rem;text-align:center;font-weight:900}
    .nv-sidebar-nav{scrollbar-width:none;-ms-overflow-style:none}.nv-sidebar-nav::-webkit-scrollbar{display:none;width:0;height:0}
    .nv-sidebar-utility{padding:9px;border:1px solid var(--border);border-radius:24px;background:color-mix(in srgb,var(--surface) 87%,var(--nvent-blue) 6%);box-shadow:0 14px 34px color-mix(in srgb,var(--primary) 10%,transparent)}
    .nv-weather-card{position:relative;overflow:hidden;min-height:132px;border-radius:18px;color:white;padding:13px;background-image:url('/assets/nvent/images/places/beach.webp');background-size:cover;background-position:center 58%;box-shadow:inset 0 1px 0 rgba(255,255,255,.25),0 10px 26px rgba(15,23,56,.16)}
    .nv-weather-card-mobile{min-height:92px;background-position:center 55%}
    .nv-weather-shade{position:absolute;inset:0;background:linear-gradient(180deg,rgba(10,18,48,.10),rgba(10,18,48,.74)),linear-gradient(90deg,rgba(12,26,66,.34),transparent 65%)}
    .nv-theme-toggle{width:100%;min-height:44px;border:1px solid var(--border);border-radius:14px;padding:.62rem .72rem;display:flex;align-items:center;gap:.65rem;background:var(--surface);color:var(--text);cursor:pointer}.nv-theme-toggle:hover{transform:translateY(-1px);border-color:color-mix(in srgb,var(--focus) 50%,var(--border));box-shadow:0 8px 20px color-mix(in srgb,var(--primary) 10%,transparent)}
    .nv-theme-toggle-icon{width:28px;height:28px;display:grid;place-items:center;border-radius:10px;background:var(--surface-2);font-size:.9rem}
  `]
})
export class AppSidebarComponent{
  @Input() workspace:'customer'|'organizer'|'venue-owner'|'admin'='customer';
  @ViewChild('mobileDrawer') mobileDrawer?:ElementRef<HTMLElement>;
  private router=inject(Router);
  private drawerTrigger:HTMLElement|null=null;
  auth=inject(AuthService);
  theme=inject(ThemeService);
  notices=inject(NotificationsService);
  weather=inject(LocalWeatherService);
  drawer=signal(false);
  query='';

  constructor(){this.weather.load();}

  get menu(){return this.workspace==='organizer'?organizer:this.workspace==='venue-owner'?venue:this.workspace==='admin'?admin:customer;}
  get home(){return this.workspace==='customer'?'/app/home':this.workspace==='organizer'?'/organizer':this.workspace==='venue-owner'?'/venue-owner':'/admin';}
  get profilePath(){return this.workspace==='customer'?'/app/profile':`${this.home}/profile`;}
  get notificationsPath(){return this.workspace==='customer'?'/app/notifications':this.workspace==='organizer'?'/organizer/notifications':this.workspace==='venue-owner'?'/venue-owner/notifications':'/admin/notifications';}
  get searchPlaceholder(){return this.workspace==='customer'?'Search events':this.workspace==='organizer'?'Search my events':this.workspace==='venue-owner'?'Search my venues':'Search users';}
  get bottomMenu():MenuItem[]{return this.workspace==='customer'?[customer[0],customer[1],customer[3],{label:'Tickets',path:'/app/tickets',icon:'⌗'},customer[8]]:[];}
  get day(){return new Intl.DateTimeFormat('en',{weekday:'short'}).format(new Date());}
  get date(){return new Intl.DateTimeFormat('en',{day:'numeric',month:'short'}).format(new Date());}
  get initials(){const n=this.auth.displayName();return n==='Guest'?'G':n.split(' ').map(x=>x[0]).slice(0,2).join('').toUpperCase();}
  get role(){return this.auth.isAuthenticated()?roleLabel(this.auth.role()):'Guest';}
  get temperature(){const value=this.weather.state().temperatureC;return value===null?(this.weather.state().loading?'…':'—°C'):`${value}°C`;}
  get weatherCondition(){return this.weather.state().loading&&!this.weather.state().available?'Checking weather…':this.weather.state().condition;}
  get weatherIcon(){return this.weather.state().icon;}

  search(){const q=this.query.trim();const path=this.workspace==='customer'?'/app/events':this.workspace==='organizer'?'/organizer/events':this.workspace==='venue-owner'?'/venue-owner/venues':'/admin/users';void this.router.navigate([path],{queryParams:q?{q}:{}});}
  openDrawer(){this.drawerTrigger=document.activeElement instanceof HTMLElement?document.activeElement:null;this.drawer.set(true);setTimeout(()=>{const root=this.mobileDrawer?.nativeElement;const target=root?.querySelector<HTMLElement>('button,a,input,select,textarea,[tabindex]:not([tabindex="-1"])');target?.focus();});}
  closeDrawer(){if(!this.drawer())return;this.drawer.set(false);const target=this.drawerTrigger;this.drawerTrigger=null;setTimeout(()=>target?.focus());}
  @HostListener('document:keydown',['$event']) onDocumentKeydown(event:KeyboardEvent){
    if(!this.drawer())return;
    if(event.key==='Escape'){event.preventDefault();this.closeDrawer();return;}
    if(event.key!=='Tab')return;
    const root=this.mobileDrawer?.nativeElement;if(!root)return;
    const items=Array.from(root.querySelectorAll<HTMLElement>('button:not([disabled]),a[href],input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])')).filter(el=>el.offsetParent!==null);
    if(!items.length){event.preventDefault();return;}
    const first=items[0],last=items[items.length-1],active=document.activeElement;
    if(event.shiftKey&&active===first){event.preventDefault();last.focus();}else if(!event.shiftKey&&active===last){event.preventDefault();first.focus();}
  }
  logout(){this.closeDrawer();this.auth.logout().subscribe({error:()=>this.auth.clear()});}
}
