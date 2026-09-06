import { RenderMode, ServerRoute } from '@angular/ssr';

// Authenticated workspaces depend on browser-local JWT/refresh-token storage.
// Rendering those routes on the server cannot see localStorage and previously
// caused hard-refreshes to bounce users to /login. Keep public pages SSR while
// rendering protected workspaces on the client.
export const serverRoutes: ServerRoute[] = [
  { path: 'customer/**', renderMode: RenderMode.Client },
  { path: 'organizer/**', renderMode: RenderMode.Client },
  { path: 'venue-owner/**', renderMode: RenderMode.Client },
  { path: 'admin/**', renderMode: RenderMode.Client },
  { path: '**', renderMode: RenderMode.Server },
];
