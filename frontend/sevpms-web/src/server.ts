import {
  AngularNodeAppEngine,
  createNodeRequestHandler,
  isMainModule,
  writeResponseToNodeResponse,
} from '@angular/ssr/node';
import express from 'express';
import http, { IncomingMessage } from 'node:http';
import https from 'node:https';
import net from 'node:net';
import tls from 'node:tls';
import { Socket } from 'node:net';
import { join } from 'node:path';

const browserDistFolder = join(import.meta.dirname, '../browser');
const backendOrigin = new URL(process.env['BACKEND_ORIGIN'] || 'http://localhost:5090');

const app = express();
const angularApp = new AngularNodeAppEngine();

const requestOrigin = (req: express.Request): string => {
  const configured = (process.env['PUBLIC_ORIGIN'] || '').replace(/\/$/, '');
  if (configured) return configured;
  const forwardedProto = String(req.headers['x-forwarded-proto'] || req.protocol).split(',')[0].trim();
  const forwardedHost = String(req.headers['x-forwarded-host'] || req.headers.host || 'localhost').split(',')[0].trim();
  return `${forwardedProto}://${forwardedHost}`;
};

app.get('/robots.txt', (req, res) => {
  const origin = requestOrigin(req);
  res.type('text/plain').send(`User-agent: *
Allow: /
Sitemap: ${origin}/sitemap.xml
`);
});

app.get('/sitemap.xml', (req, res) => {
  const origin = requestOrigin(req);
  const paths = ['/', '/events', '/venues', '/services', '/about', '/privacy', '/terms'];
  const urls = paths.map((path) => `  <url><loc>${origin}${path}</loc></url>`).join('\n');
  res.type('application/xml').send(`<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
${urls}
</urlset>
`);
});

/**
 * Production same-origin proxy for API and SignalR negotiate requests.
 * Set BACKEND_ORIGIN to the ASP.NET Core origin, for example http://127.0.0.1:5090.
 */
app.use(['/api', '/hubs'], (req, res) => {
  const target = new URL(req.originalUrl, backendOrigin);
  const client = target.protocol === 'https:' ? https : http;
  const proxyReq = client.request(
    target,
    {
      method: req.method,
      headers: {
        ...req.headers,
        host: target.host,
        'x-forwarded-host': req.headers.host ?? '',
        'x-forwarded-proto': req.protocol,
      },
    },
    (proxyRes) => {
      res.status(proxyRes.statusCode ?? 502);
      for (const [name, value] of Object.entries(proxyRes.headers)) {
        if (value !== undefined) res.setHeader(name, value);
      }
      proxyRes.pipe(res);
    },
  );
  proxyReq.on('error', () => {
    if (!res.headersSent) res.status(502).json({ error: 'backend_unavailable' });
    else res.end();
  });
  req.pipe(proxyReq);
});

app.use(
  express.static(browserDistFolder, {
    maxAge: '1y',
    index: false,
    redirect: false,
  }),
);

app.use((req, res, next) => {
  angularApp
    .handle(req)
    .then((response) =>
      response ? writeResponseToNodeResponse(response, res) : next(),
    )
    .catch(next);
});

function proxyWebSocket(req: IncomingMessage, clientSocket: Socket, head: Buffer): void {
  const requestUrl = req.url ?? '/';
  if (!requestUrl.startsWith('/hubs/')) {
    clientSocket.destroy();
    return;
  }

  const port = Number(backendOrigin.port || (backendOrigin.protocol === 'https:' ? 443 : 80));
  const onConnected = (upstream: net.Socket | tls.TLSSocket): void => {
    const lines = [`${req.method ?? 'GET'} ${requestUrl} HTTP/1.1`];
    for (const [name, value] of Object.entries(req.headers)) {
      if (value === undefined) continue;
      if (name.toLowerCase() === 'host') {
        lines.push(`host: ${backendOrigin.host}`);
      } else if (Array.isArray(value)) {
        for (const item of value) lines.push(`${name}: ${item}`);
      } else {
        lines.push(`${name}: ${value}`);
      }
    }
    lines.push(`x-forwarded-host: ${req.headers.host ?? ''}`);
    lines.push('');
    lines.push('');
    upstream.write(lines.join('\r\n'));
    if (head.length) upstream.write(head);
    clientSocket.pipe(upstream).pipe(clientSocket);
  };

  const upstream = backendOrigin.protocol === 'https:'
    ? tls.connect({ host: backendOrigin.hostname, port, servername: backendOrigin.hostname }, () => onConnected(upstream))
    : net.connect({ host: backendOrigin.hostname, port }, () => onConnected(upstream));

  upstream.on('error', () => clientSocket.destroy());
  clientSocket.on('error', () => upstream.destroy());
}

if (isMainModule(import.meta.url) || process.env['pm_id']) {
  const port = Number(process.env['PORT'] || 4000);
  const server = app.listen(port, (error) => {
    if (error) throw error;
    console.log(`Node Express server listening on http://localhost:${port}`);
  });
  server.on('upgrade', proxyWebSocket);
}

export const reqHandler = createNodeRequestHandler(app);
