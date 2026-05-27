import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.accessToken();
  const isAuthRequest = req.url.includes('/api/auth');
  const isAdminRequest = req.url.includes('/api/admin');

  if (isAuthRequest) {
    return next(req.clone({ withCredentials: true }));
  }

  if (!token || !isAdminRequest) {
    return next(req);
  }

  const authorizedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`,
    },
  });

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || req.headers.has('x-auth-retry')) {
        return throwError(() => error);
      }

      return from(auth.refreshSession()).pipe(
        switchMap((result) => {
          if (result.error || !auth.accessToken()) {
            auth.clearSession();
            return throwError(() => error);
          }

          return next(
            req.clone({
              headers: req.headers.set('x-auth-retry', 'true'),
              setHeaders: {
                Authorization: `Bearer ${auth.accessToken()}`,
              },
            }),
          );
        }),
      );
    }),
  );
};
