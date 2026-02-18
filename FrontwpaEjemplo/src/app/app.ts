import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';
import { SyncStatus } from './sync-status/sync-status';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, CommonModule, SyncStatus],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected authService = inject(AuthService);
}
