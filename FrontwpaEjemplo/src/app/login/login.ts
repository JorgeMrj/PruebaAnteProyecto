import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  username = signal('');
  password = signal('');
  loading = signal(false);
  errorMessage = signal('');

  onSubmit(): void {
    if (!this.username() || !this.password()) {
      this.errorMessage.set('Por favor, completa todos los campos');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.authService
      .login({
        username: this.username(),
        password: this.password(),
      })
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.router.navigate(['/']);
        },
        error: (error) => {
          this.loading.set(false);
          console.error('Login error:', error);

          // Manejo específico de errores
          if (error.status === 0) {
            this.errorMessage.set('No se puede conectar con el servidor. Verifica tu conexión.');
          } else if (error.status === 500) {
            this.errorMessage.set('Error del servidor. Por favor, contacta al administrador.');
          } else if (error.status === 401 || error.status === 403) {
            this.errorMessage.set('Usuario o contraseña incorrectos.');
          } else {
            this.errorMessage.set(
              error.error?.message || 'Error al iniciar sesión. Verifica tus credenciales.',
            );
          }
        },
      });
  }
}
