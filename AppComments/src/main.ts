import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { importProvidersFrom } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';

import { provideRouter } from '@angular/router';
import {appRouting, routes} from './app/app.routes';


bootstrapApplication(AppComponent, {
  providers: [
    appRouting,
    provideHttpClient()
  ]
});
