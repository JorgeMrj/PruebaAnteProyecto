import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SyncService } from '../services/sync.service';

@Component({
  selector: 'app-sync-status',
  imports: [CommonModule],
  template: `
    <div class="fixed bottom-4 right-4 z-50">
      <!-- Badge de estado de conexión -->
      @if (!syncService.isOnline()) {
        <div class="alert alert-warning shadow-lg mb-2 max-w-sm">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 5.636a9 9 0 010 12.728m0 0l-2.829-2.829m2.829 2.829L21 21M15.536 8.464a5 5 0 010 7.072m0 0l-2.829-2.829m-4.243 2.829a4.978 4.978 0 01-1.414-2.83m-1.414 5.658a9 9 0 01-2.167-9.238m7.824 2.167a1 1 0 111.414 1.414m-1.414-1.414L3 3" />
          </svg>
          <div>
            <h3 class="font-bold">Sin conexión</h3>
            <div class="text-xs">Modo offline activado</div>
          </div>
        </div>
      }

      <!-- Badge de operaciones pendientes -->
      @if (syncService.pendingCount() > 0) {
        <div class="alert alert-info shadow-lg max-w-sm">
          <svg xmlns="http://www.w3.org/2000/svg" class="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <h3 class="font-bold">{{ syncService.pendingCount() }} operaciones pendientes</h3>
            <div class="text-xs">Se sincronizarán cuando haya conexión</div>
          </div>
          @if (syncService.isOnline() && !syncService.isSyncing()) {
            <button class="btn btn-sm btn-ghost" (click)="syncNow()">
              Sincronizar ahora
            </button>
          }
        </div>
      }

      <!-- Indicador de sincronización -->
      @if (syncService.isSyncing()) {
        <div class="alert alert-info shadow-lg max-w-sm">
          <span class="loading loading-spinner loading-md"></span>
          <div>
            <h3 class="font-bold">Sincronizando...</h3>
            <div class="text-xs">Subiendo cambios al servidor</div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `],
})
export class SyncStatus {
  protected syncService = inject(SyncService);

  syncNow(): void {
    this.syncService.syncPendingOperations();
  }
}
