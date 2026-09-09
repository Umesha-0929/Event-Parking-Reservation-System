import { SessionService } from './session.service';

describe('SessionService', () => {
  beforeEach(() => {
    localStorage.clear();
    sessionStorage.clear();
  });

  it('keeps access credentials out of localStorage', () => {
    localStorage.setItem('nvent.accessToken', 'legacy-access');
    localStorage.setItem('nvent.refreshToken', 'legacy-refresh');
    const service = new SessionService();
    service.saveAuth({ accessToken: 'memory-only-token', user: { email: 'user@example.test', role: 'Customer' } });

    expect(service.accessToken()).toBe('memory-only-token');
    expect(localStorage.getItem('nvent.accessToken')).toBeNull();
    expect(localStorage.getItem('nvent.refreshToken')).toBeNull();
  });

  it('maps every backend role to the correct workspace', () => {
    const service = new SessionService();
    expect(service.routeForRole('Customer')).toBe('/customer/dashboard');
    expect(service.routeForRole('EventOrganizer')).toBe('/organizer/dashboard');
    expect(service.routeForRole('VenueOwner')).toBe('/venue-owner/dashboard');
    expect(service.routeForRole('Admin')).toBe('/admin/dashboard');
  });

  it('clears the in-memory session and cached user state', () => {
    const service = new SessionService();
    service.saveAuth({ accessToken: 'token', user: { email: 'admin@example.test', role: 'Admin' } });
    expect(service.hasSession()).toBeTrue();
    service.clear();
    expect(service.accessToken()).toBeNull();
    expect(service.user()).toBeNull();
    expect(service.hasSession()).toBeFalse();
  });
});
