import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { EventSummary } from '../../../core/models/api.models';
import { httpErrorMessage } from '../../../core/utils/http-error';
@Component({selector:'app-organizer-dashboard',imports:[RouterLink],templateUrl:'./organizer-dashboard.html',styleUrl:'./organizer-dashboard.scss'})
export class OrganizerDashboardComponent implements OnInit{
 private readonly domain=inject(DomainApiService); readonly events=signal<EventSummary[]>([]);readonly loading=signal(true);readonly error=signal('');
 ngOnInit():void{this.load();} load():void{this.loading.set(true);this.domain.myEvents().subscribe({next:(events)=>{this.events.set(events);this.loading.set(false);},error:(error)=>{this.error.set(httpErrorMessage(error,'Organizer events could not be loaded.'));this.loading.set(false);}});}
 readonly published=()=>this.events().filter((e)=>String(e.status).toLowerCase().includes('publish')||String(e.status)==='1').length;
}
