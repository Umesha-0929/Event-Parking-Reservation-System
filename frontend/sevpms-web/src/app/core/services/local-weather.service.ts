import { Injectable, signal } from '@angular/core';

export interface LocalWeatherState {
  temperatureC:number|null;
  condition:string;
  icon:string;
  isDay:boolean;
  available:boolean;
  loading:boolean;
}

@Injectable({providedIn:'root'})
export class LocalWeatherService {
  readonly state=signal<LocalWeatherState>({temperatureC:null,condition:'Local conditions',icon:'☀',isDay:true,available:false,loading:false});
  private started=false;
  private readonly cacheKey='nvent_local_weather_v1';

  load(){
    if(this.started)return;
    this.started=true;
    this.restoreCache();
    if(typeof navigator==='undefined'||!navigator.geolocation)return;
    this.state.update(v=>({...v,loading:true}));
    navigator.geolocation.getCurrentPosition(
      position=>void this.fetchCurrent(position.coords.latitude,position.coords.longitude),
      ()=>this.state.update(v=>({...v,loading:false})),
      {enableHighAccuracy:false,maximumAge:15*60*1000,timeout:6000}
    );
  }

  private async fetchCurrent(latitude:number,longitude:number){
    try{
      const params=new URLSearchParams({
        latitude:String(latitude),
        longitude:String(longitude),
        current:'temperature_2m,weather_code,is_day',
        timezone:'auto'
      });
      const response=await fetch(`https://api.open-meteo.com/v1/forecast?${params.toString()}`,{headers:{Accept:'application/json'}});
      if(!response.ok)throw new Error('weather_unavailable');
      const json=await response.json() as {current?:{temperature_2m?:number;weather_code?:number;is_day?:number}};
      const current=json.current;
      if(!current||typeof current.temperature_2m!=='number')throw new Error('weather_invalid');
      const mapped=this.mapCode(Number(current.weather_code??0),Number(current.is_day??1)===1);
      const next:LocalWeatherState={temperatureC:Math.round(current.temperature_2m),condition:mapped.condition,icon:mapped.icon,isDay:Number(current.is_day??1)===1,available:true,loading:false};
      this.state.set(next);
      localStorage.setItem(this.cacheKey,JSON.stringify({...next,savedAt:Date.now()}));
    }catch{
      this.state.update(v=>({...v,loading:false}));
    }
  }

  private restoreCache(){
    try{
      const raw=localStorage.getItem(this.cacheKey);if(!raw)return;
      const cached=JSON.parse(raw) as LocalWeatherState&{savedAt?:number};
      if(!cached.savedAt||Date.now()-cached.savedAt>60*60*1000)return;
      this.state.set({...cached,loading:false});
    }catch{ /* ignore invalid cache */ }
  }

  private mapCode(code:number,isDay:boolean){
    if(code===0)return{condition:'Clear skies',icon:isDay?'☀':'☾'};
    if(code<=2)return{condition:'Mostly clear',icon:isDay?'🌤':'☾'};
    if(code===3)return{condition:'Cloudy',icon:'☁'};
    if(code===45||code===48)return{condition:'Misty',icon:'🌫'};
    if([51,53,55,56,57].includes(code))return{condition:'Light drizzle',icon:'🌦'};
    if([61,63,65,66,67,80,81,82].includes(code))return{condition:'Rain showers',icon:'🌧'};
    if([71,73,75,77,85,86].includes(code))return{condition:'Snow showers',icon:'❄'};
    if([95,96,99].includes(code))return{condition:'Thunderstorms',icon:'⛈'};
    return{condition:'Local conditions',icon:isDay?'☀':'☾'};
  }
}
