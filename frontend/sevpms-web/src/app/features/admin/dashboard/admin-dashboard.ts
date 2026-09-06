import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AdminDashboardStatsDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
@Component({selector:'app-admin-dashboard',imports:[RouterLink],templateUrl:'./admin-dashboard.html',styleUrl:'./admin-dashboard.scss'})
export class AdminDashboardComponent implements OnInit{private readonly domain=inject(DomainApiService);readonly stats=signal<AdminDashboardStatsDto>({});readonly loading=signal(true);readonly error=signal('');ngOnInit():void{this.load();}load():void{this.loading.set(true);this.domain.adminStats().subscribe({next:(s)=>{this.stats.set(s);this.loading.set(false);},error:(e)=>{this.error.set(httpErrorMessage(e,'Admin statistics could not be loaded.'));this.loading.set(false);}});} value(...keys:string[]):string{for(const key of keys){const v=this.stats()[key];if(v!==undefined&&v!==null)return typeof v==='number'?v.toLocaleString():String(v);}return '—';}}
