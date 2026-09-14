import { Component, OnDestroy, inject } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { Subscription, filter } from 'rxjs';
import { NetworkStatusService } from './core/services/network-status.service';

@Component({
  selector:'app-root',
  standalone:true,
  imports:[RouterOutlet],
  template:`
    <a href="#main-content" class="skip-link">Skip to main content</a>
    @if(!network.online()){
      <div class="fixed top-3 left-1/2 -translate-x-1/2 z-[120] max-w-[calc(100vw-2rem)] nv-card px-4 py-3 text-sm font-bold" role="status" aria-live="polite">
        You're offline. Previously loaded information may be out of date, and bookings, payments, parking and food actions require a connection.
      </div>
    } @else if(network.apiAvailable()===false){
      <div class="fixed top-3 left-1/2 -translate-x-1/2 z-[120] max-w-[calc(100vw-2rem)] nv-card px-4 py-3 text-sm" role="status" aria-live="polite">
        <span class="font-bold">Nvent services are temporarily unavailable.</span>
        <button type="button" class="ml-2 font-extrabold underline underline-offset-4" (click)="network.checkApi()">Retry</button>
      </div>
    }
    <router-outlet/>
  `
})
export class AppComponent implements OnDestroy{
  network=inject(NetworkStatusService);
  private router=inject(Router);
  private navigationFocus:Subscription;
  private mediaObserver?:MutationObserver;
  private readonly enforceMute=(event:Event)=>{
    if(event.target instanceof HTMLVideoElement)this.muteVideo(event.target);
  };

  constructor(){
    this.navigationFocus=this.router.events.pipe(filter((event):event is NavigationEnd=>event instanceof NavigationEnd)).subscribe(()=>{
      setTimeout(()=>{
        document.getElementById('main-content')?.focus();
        this.muteAllVideos();
      },0);
    });
    this.installGlobalVideoMute();
  }

  ngOnDestroy(){
    this.navigationFocus.unsubscribe();
    this.mediaObserver?.disconnect();
    document.removeEventListener('volumechange',this.enforceMute,true);
  }

  private installGlobalVideoMute(){
    this.muteAllVideos();
    document.addEventListener('volumechange',this.enforceMute,true);
    if(typeof MutationObserver==='undefined')return;
    this.mediaObserver=new MutationObserver(records=>{
      for(const record of records){
        for(const node of Array.from(record.addedNodes)){
          if(node instanceof HTMLVideoElement)this.muteVideo(node);
          else if(node instanceof HTMLElement)node.querySelectorAll('video').forEach(video=>this.muteVideo(video));
        }
      }
    });
    this.mediaObserver.observe(document.documentElement,{childList:true,subtree:true});
  }

  private muteAllVideos(){document.querySelectorAll('video').forEach(video=>this.muteVideo(video));}

  private muteVideo(video:HTMLVideoElement){
    if(!video.muted)video.muted=true;
    if(!video.defaultMuted)video.defaultMuted=true;
    if(video.volume!==0)video.volume=0;
    if(!video.hasAttribute('muted'))video.setAttribute('muted','');
  }
}
