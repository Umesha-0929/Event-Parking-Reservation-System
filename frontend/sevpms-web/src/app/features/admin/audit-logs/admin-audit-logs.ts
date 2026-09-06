import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuditLogDto } from '../../../core/models/api.models';
import { DomainApiService } from '../../../core/services/domain-api.service';
import { httpErrorMessage } from '../../../core/utils/http-error';
@Component({selector:'app-admin-audit-logs',imports:[FormsModule],templateUrl:'./admin-audit-logs.html',styleUrl:'./admin-audit-logs.scss'})
export class AdminAuditLogsComponent implements OnInit{private readonly domain=inject(DomainApiService);readonly logs=signal<AuditLogDto[]>([]);readonly loading=signal(true);readonly error=signal('');query='';ngOnInit():void{this.load();}load():void{this.loading.set(true);this.error.set('');this.domain.auditLogs().subscribe({next:l=>{this.logs.set(l);this.loading.set(false);},error:e=>{this.loading.set(false);this.error.set(httpErrorMessage(e,'Audit activity could not be loaded.'));}});}filtered():AuditLogDto[]{const q=this.query.trim().toLowerCase();return this.logs().filter(x=>!q||`${x.action} ${x.entityType} ${x.entityId}`.toLowerCase().includes(q));}}
