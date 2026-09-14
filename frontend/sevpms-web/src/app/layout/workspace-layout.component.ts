import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterOutlet } from '@angular/router';
import { AppSidebarComponent } from '../shared/components/app-sidebar.component';
import { NotificationsService } from '../core/services/notifications.service';
import { AuthService } from '../core/services/auth.service';
import { RealtimeService } from '../core/services/realtime.service';

@Component({
  selector:'app-workspace-layout',
  standalone:true,
  imports:[RouterOutlet,AppSidebarComponent],
  template:`<app-sidebar [workspace]="workspace"/><main id="main-content" tabindex="-1" class="lg:ml-[258px] pt-16 lg:pt-0 min-h-screen"><router-outlet/></main>`
})
export class WorkspaceLayoutComponent implements OnInit,OnDestroy{
  private route=inject(ActivatedRoute);
  private notices=inject(NotificationsService);
  private auth=inject(AuthService);
  private realtime=inject(RealtimeService);
  workspace=this.route.snapshot.data['workspace'] as 'customer'|'organizer'|'venue-owner'|'admin';
  ngOnInit(){if(this.auth.isAuthenticated()){this.notices.list().subscribe({error:()=>void 0});void this.realtime.start();}}
  ngOnDestroy(){void this.realtime.stop();}
}
