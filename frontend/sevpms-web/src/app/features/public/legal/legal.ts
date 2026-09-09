import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SeoService } from '../../../core/services/seo.service';
import { RevealOnScrollDirective } from '../../../shared/directives/reveal-on-scroll.directive';

@Component({ selector: 'app-legal', imports: [RevealOnScrollDirective], templateUrl: './legal.html', styleUrl: './legal.scss' })
export class LegalComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly seo = inject(SeoService);
  readonly kind = (this.route.snapshot.data['kind'] ?? 'privacy') as 'privacy' | 'terms';
  readonly title = this.kind === 'privacy' ? 'Privacy Policy' : 'Terms of Use';
  readonly effectiveDate = '8 September 2026';
  ngOnInit(): void {
    this.seo.setPage(this.title, this.kind === 'privacy'
      ? 'How Nvent handles account, booking, payment, event, venue and service data.'
      : 'Rules for using Nvent event, venue, ticket, parking and related services.');
  }
}
