import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.dirname(fileURLToPath(import.meta.url));
const frontendRoot = path.resolve(here, '..');
const backendRoot = path.resolve(frontendRoot, '../../backend/src/SEVPMS.Api');
const failures = [];
const checks = [];

function read(relativePath) {
  const file = path.join(backendRoot, relativePath);
  if (!fs.existsSync(file)) {
    failures.push(`Missing backend security source: ${relativePath}`);
    return '';
  }
  return fs.readFileSync(file, 'utf8');
}
function expect(label, condition) {
  checks.push(label);
  if (!condition) failures.push(label);
}

const tickets = read('Controllers/Klegar/TicketsController.cs');
expect('Tickets controller requires authentication', /\[Authorize\][\s\S]*class\s+TicketsController/.test(tickets));
expect('Booking ticket reads enforce ownership/role access', tickets.includes('CanAccessBookingAsync(HttpContext, bookingId'));
expect('Ticket-number reads enforce ownership/role access', tickets.includes('CanAccessTicketAsync(HttpContext, ticketNo'));
expect('Ticket issue enforces booking/event ownership', tickets.includes('CanManageBookingAsync(HttpContext, bookingId, request.EventId'));
expect('Ticket cancellation enforces ticket ownership', tickets.includes('CanManageTicketAsync(HttpContext, ticketNo'));

const seating = read('Controllers/Klegar/SeatingLayoutsController.cs');
const seatingOwnershipChecks = [...seating.matchAll(/CanManageEventAsync\(HttpContext, eventId/g)].length;
expect('Organizer seating mutations/reads enforce event ownership', seatingOwnershipChecks >= 6);

const resolver = read('Klegar/RequestUserResolver.cs');
expect('Request identity resolver has no development impersonation headers', !/X-SEVPMS-Demo|AllowDevelopmentUserHeader/i.test(resolver));
expect('Request identity comes from authenticated claims', /FindFirstValue\(ClaimTypes\.NameIdentifier\)/.test(resolver));

const program = read('Program.cs');
expect('JWT signing key is mandatory', /Jwt:Key[^\n]*throw new InvalidOperationException/.test(program));
expect('SignalR access tokens are limited to known hub paths', program.includes('path.StartsWithSegments("/hubs/notifications")') && program.includes('path.StartsWithSegments("/hubs/events")'));
expect('Security headers include nosniff', program.includes('X-Content-Type-Options') && program.includes('nosniff'));
expect('Security headers include frame denial', program.includes('X-Frame-Options') && program.includes('DENY'));
expect('Production SPA fallback excludes API requests', program.includes('path.StartsWithSegments("/api")'));
expect('Production SPA fallback excludes hub requests', program.includes('path.StartsWithSegments("/hubs")'));
expect('Production SPA fallback only handles GET/HEAD navigation', program.includes('HttpMethods.IsGet') && program.includes('HttpMethods.IsHead'));
expect('CORS credentials are only configured for explicit origins', /if \(allowedOrigins\.Length > 0\)[\s\S]{0,350}AllowCredentials\(\)/.test(program));
expect('Global rate limiter is configured', program.includes('AddRateLimiter') && program.includes('UseRateLimiter'));

console.log('Nvent backend security-contract verification');
console.log(`  Checks executed: ${checks.length}`);
if (failures.length) {
  console.error(`\nFAILED (${failures.length})`);
  for (const failure of failures) console.error(`  - ${failure}`);
  process.exitCode = 1;
} else {
  console.log('\nPASS: critical API-boundary security guards are present.');
}
