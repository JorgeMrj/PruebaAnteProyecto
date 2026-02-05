import { provideServiceWorker } from '@angular/service-worker';
import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app';

bootstrapApplication(App, {
  providers: [
    provideServiceWorker('ngsw-worker.js', { enabled: true })
  ]
})
  .catch(err => console.error(err));
