import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

@Injectable({ providedIn: 'root' })
export class SeoService {
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);

  setPage(title: string, description: string, image = '/assets/images/concert-poster.jpg'): void {
    const pageTitle = title.replace(/\s*\|\s*Nvent\s*$/i, '').replace(/^Nvent\s*\|\s*/i, '').trim();
    const fullTitle = pageTitle ? `Nvent | ${pageTitle}` : 'Nvent';
    this.title.setTitle(fullTitle);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ property: 'og:title', content: fullTitle });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:image', content: image });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
  }
}
