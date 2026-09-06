import { HttpErrorResponse } from '@angular/common/http';
import { ApiErrorShape } from '../models/api.models';

export function httpErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  const httpError = error as HttpErrorResponse;
  const body = (httpError.error ?? {}) as ApiErrorShape | string;
  if (typeof body === 'string' && body.trim()) return body;
  if (typeof body === 'object' && body) {
    if (body.detail) return body.detail;
    if (body.message) return body.message;
    if (body.title) return body.title;
    const first = Object.values(body.errors ?? {})[0]?.[0];
    if (first) return first;
  }
  if (httpError.status === 0) return 'Nvent services are temporarily unavailable. Please try again in a moment.';
  return fallback;
}
