export type UserRole = 'guest' | 'customer' | 'organizer' | 'venue-owner' | 'admin';
export type SeatStatus = 'Available' | 'Selected' | 'Held' | 'Booked' | 'Unavailable';

export interface NavItem {
  label: string;
  path: string;
  icon?: string;
}

export interface PreviewCard {
  eyebrow: string;
  title: string;
  copy: string;
  image?: string;
  badge?: string;
  meta?: string;
  path?: string;
}

export interface SeatViewModel {
  id: string;
  row: string;
  number: number;
  category: string;
  price: number;
  status: SeatStatus;
  has360: boolean;
}
