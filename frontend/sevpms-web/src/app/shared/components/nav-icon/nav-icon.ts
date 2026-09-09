import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-nav-icon',
  template: `
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
      @switch (name) {
        @case ('home') { <path d="M3 10.8 12 3l9 7.8"/><path d="M5.5 9.5V21h13V9.5"/><path d="M9.5 21v-6h5v6"/> }
        @case ('calendar') { <rect x="3" y="5" width="18" height="16" rx="2"/><path d="M7 3v4M17 3v4M3 10h18"/><path d="M8 14h2M14 14h2M8 18h2M14 18h2"/> }
        @case ('plus-circle') { <circle cx="12" cy="12" r="9"/><path d="M12 8v8M8 12h8"/> }
        @case ('building') { <path d="M4 21V6l8-3 8 3v15"/><path d="M8 8h2M14 8h2M8 12h2M14 12h2M8 16h2M14 16h2M3 21h18"/> }
        @case ('seat') { <path d="M6 4v9a3 3 0 0 0 3 3h8"/><path d="M6 10h9a3 3 0 0 1 3 3v8M4 21h16"/><path d="M10 6h6"/> }
        @case ('tag') { <path d="M3 12V5a2 2 0 0 1 2-2h7l9 9-9 9-9-9Z"/><circle cx="8" cy="8" r="1.2"/> }
        @case ('qr') { <rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><path d="M14 14h3v3h-3zM18 18h3v3M18 14h3M14 19v2"/> }
        @case ('chart') { <path d="M4 20V10M10 20V4M16 20v-7M22 20H2"/> }
        @case ('bell') { <path d="M18 8a6 6 0 1 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9"/><path d="M10 21h4"/> }
        @case ('settings') { <circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-2.8 2.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.6v.2h-4v-.2a1.7 1.7 0 0 0-1-1.6 1.7 1.7 0 0 0-1.9.3l-.1.1L4.2 17l.1-.1a1.7 1.7 0 0 0 .3-1.9A1.7 1.7 0 0 0 3 14H2.8v-4H3a1.7 1.7 0 0 0 1.6-1 1.7 1.7 0 0 0-.3-1.9L4.2 7 7 4.2l.1.1A1.7 1.7 0 0 0 9 4.6a1.7 1.7 0 0 0 1-1.6v-.2h4V3a1.7 1.7 0 0 0 1 1.6 1.7 1.7 0 0 0 1.9-.3l.1-.1L19.8 7l-.1.1a1.7 1.7 0 0 0-.3 1.9 1.7 1.7 0 0 0 1.6 1h.2v4H21a1.7 1.7 0 0 0-1.6 1Z"/> }
        @case ('ticket') { <path d="M4 6h16v4a2 2 0 0 0 0 4v4H4v-4a2 2 0 0 0 0-4V6Z"/><path d="M12 8v8"/> }
        @case ('parking') { <rect x="4" y="3" width="16" height="18" rx="2"/><path d="M9 17V7h4a3 3 0 1 1 0 6H9"/> }
        @case ('food') { <path d="M7 3v7M4 3v5a3 3 0 0 0 6 0V3M7 10v11M16 3v18M16 3c3 2 4 5 4 8h-4"/> }
        @case ('pin') { <path d="M20 10c0 5-8 11-8 11S4 15 4 10a8 8 0 1 1 16 0Z"/><circle cx="12" cy="10" r="2.5"/> }
        @case ('user') { <circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/> }
        @case ('check') { <circle cx="12" cy="12" r="9"/><path d="m8 12 2.5 2.5L16.5 8.5"/> }
        @case ('clock') { <circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/> }
        @case ('market') { <path d="M3 9h18l-2-5H5L3 9Z"/><path d="M5 9v11h14V9M9 20v-6h6v6"/><path d="M3 9a3 3 0 0 0 6 0 3 3 0 0 0 6 0 3 3 0 0 0 6 0"/> }
        @case ('list') { <path d="M9 6h11M9 12h11M9 18h11"/><circle cx="4.5" cy="6" r="1"/><circle cx="4.5" cy="12" r="1"/><circle cx="4.5" cy="18" r="1"/> }
        @case ('users') { <circle cx="9" cy="8" r="3"/><circle cx="17" cy="9" r="2.5"/><path d="M3 20a6 6 0 0 1 12 0M14 16a5 5 0 0 1 7 4"/> }
        @case ('audit') { <path d="M5 3h10l4 4v14H5V3Z"/><path d="M14 3v5h5M8 12h8M8 16h5"/> }
        @case ('operations') { <path d="M4 7h16M4 17h16"/><circle cx="9" cy="7" r="2"/><circle cx="15" cy="17" r="2"/> }
        @case ('search') { <circle cx="11" cy="11" r="6"/><path d="m16 16 4 4"/> }
        @case ('menu') { <path d="M4 7h16M4 12h16M4 17h16"/> }
        @case ('logout') { <path d="M10 5H5v14h5M14 8l4 4-4 4M18 12H9"/> }
        @default { <circle cx="12" cy="12" r="8"/> }
      }
    </svg>
  `,
  styles: [':host{display:inline-grid;place-items:center;width:1.2em;height:1.2em;line-height:1}:host svg{display:block;width:100%;height:100%}'],
})
export class NavIconComponent {
  @Input() name = 'circle';
}
