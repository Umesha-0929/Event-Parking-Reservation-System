import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SeoService {
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly document = inject(DOCUMENT);
  private readonly router = inject(Router);

  setPage(title: string, description: string, image = '/assets/images/concert-poster.jpg'): void {
    const pageTitle = title.replace(/\s*\|\s*Nvent\s*$/i, '').replace(/^Nvent\s*\|\s*/i, '').trim();
    const fullTitle = pageTitle ? `Nvent | ${pageTitle}` : 'Nvent';
    const base = environment.canonicalBaseUrl.replace(/\/$/, '') || this.browserOrigin();
    const path = this.router.url.split('?')[0] || '/';
    const canonical = base ? `${base}${path}` : path;
    const imageUrl = image.startsWith('http') || !base ? image : `${base}${image.startsWith('/') ? '' : '/'}${image}`;

    this.title.setTitle(fullTitle);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ property: 'og:title', content: fullTitle });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:image', content: imageUrl });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ property: 'og:url', content: canonical });
    this.meta.updateTag({ name: 'twitter:card', content: 'summary_large_image' });
    this.meta.updateTag({ name: 'twitter:title', content: fullTitle });
    this.meta.updateTag({ name: 'twitter:description', content: description });
    this.meta.updateTag({ name: 'twitter:image', content: imageUrl });
    this.updateCanonical(canonical);
  }

  private browserOrigin(): string {
    const view = this.document.defaultView;
    return view?.location?.origin?.replace(/\/$/, '') ?? '';
  }

  private updateCanonical(href: string): void {
    let link = this.document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    if (!link) {
      link = this.document.createElement('link');
      link.rel = 'canonical';
      this.document.head.appendChild(link);
    }
    link.href = href;
  }
}
