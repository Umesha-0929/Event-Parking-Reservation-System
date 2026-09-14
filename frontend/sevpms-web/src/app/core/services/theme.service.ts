import { Injectable, signal } from '@angular/core';

export type ThemeMode='light'|'dark'|'system';

@Injectable({providedIn:'root'})
export class ThemeService {
  readonly mode=signal<ThemeMode>((localStorage.getItem('nvent_theme') as ThemeMode)||'system');
  readonly dark=signal(false);
  private readonly media=typeof matchMedia!=='undefined'?matchMedia('(prefers-color-scheme: dark)'):null;

  constructor(){
    this.apply();
    this.media?.addEventListener('change',()=>{if(this.mode()==='system')this.apply();});
  }

  set(mode:ThemeMode){
    this.mode.set(mode);
    localStorage.setItem('nvent_theme',mode);
    this.apply();
  }

  toggle(){this.set(this.dark()?'light':'dark');}

  private apply(){
    const mode=this.mode();
    const dark=mode==='dark'||(mode==='system'&&!!this.media?.matches);
    this.dark.set(dark);
    document.documentElement.classList.toggle('dark',dark);
    document.documentElement.dataset['theme']=dark?'dark':'light';
    document.documentElement.style.colorScheme=dark?'dark':'light';
  }
}
