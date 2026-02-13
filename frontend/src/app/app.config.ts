import { ApplicationConfig, LOCALE_ID, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { registerLocaleData } from '@angular/common';
import localeUk from '@angular/common/locales/uk';

import { routes } from './app.routes';
import { authInterceptor } from './auth.interceptor';

registerLocaleData(localeUk);

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor])),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    { provide: LOCALE_ID, useValue: 'uk-UA' }
  ]
};
