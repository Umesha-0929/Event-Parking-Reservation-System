import { Injectable, inject } from '@angular/core';
import { ApiService } from './api.service';
import { OrganizerReportDto, PlatformReportDto, VenueOwnerReportDto } from '../models/api.models';

@Injectable({providedIn:'root'})
export class ReportsService {
  private api=inject(ApiService);
  organizer(){return this.api.get<OrganizerReportDto>('reports/organizer');}
  venueOwner(){return this.api.get<VenueOwnerReportDto>('reports/venue-owner');}
  platform(params:Record<string,unknown>={}){return this.api.get<PlatformReportDto>('reports/platform',params);}
}
