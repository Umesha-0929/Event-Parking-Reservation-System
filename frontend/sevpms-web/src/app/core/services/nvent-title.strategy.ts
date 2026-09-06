import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterStateSnapshot, TitleStrategy } from '@angular/router';

@Injectable()
export class NventTitleStrategy extends TitleStrategy {
  constructor(private readonly titleService: Title) {
    super();
  }

  override updateTitle(routerState: RouterStateSnapshot): void {
    const pageTitle = this.buildTitle(routerState);
    this.titleService.setTitle(pageTitle ? `Nvent | ${pageTitle}` : 'Nvent');
  }
}
