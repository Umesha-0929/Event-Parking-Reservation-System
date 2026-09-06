import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { SeoService } from '../../../core/services/seo.service';
import { RevealOnScrollDirective } from '../../../shared/directives/reveal-on-scroll.directive';
@Component({selector:'app-success',imports:[RouterLink, RevealOnScrollDirective],templateUrl:'./success.html',styleUrl:'./success.scss'})
export class SuccessComponent implements OnInit{private readonly seo=inject(SeoService);ngOnInit():void{this.seo.setPage('Success','Nvent success state preview with clear next actions.');}}
