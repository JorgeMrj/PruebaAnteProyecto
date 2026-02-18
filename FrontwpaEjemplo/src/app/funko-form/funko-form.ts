import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { FunkoService, Funko } from '../services/funko.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-funko-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './funko-form.html',
  styleUrl: './funko-form.css',
})
export class FunkoForm implements OnInit {
  private funkoService = inject(FunkoService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  funkoId = signal<number | null>(null);
  isEditMode = signal(false);
  loading = signal(false);
  errorMessage = signal('');
  successMessage = signal('');

  // Campos del formulario
  nombre = signal('');
  precio = signal(0);
  categoria = signal('');
  imagen = signal('');
  selectedFile = signal<File | null>(null);
  previewUrl = signal<string | null>(null);

  ngOnInit(): void {
    // Verificar si el usuario está autenticado
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }

    // Verificar si estamos en modo edición
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.funkoId.set(+id);
      this.isEditMode.set(true);
      this.loadFunko(+id);
    }
  }

  loadFunko(id: number): void {
    this.loading.set(true);
    this.funkoService.getFunko(id).subscribe({
      next: (funko) => {
        this.nombre.set(funko.nombre);
        this.precio.set(funko.precio);
        this.categoria.set(funko.categoria);
        this.imagen.set(funko.imagen || '');
        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.errorMessage.set('Error al cargar el Funko');
        console.error('Error loading funko:', error);
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validar tipo de archivo
      if (!file.type.startsWith('image/')) {
        this.errorMessage.set('Por favor, selecciona un archivo de imagen válido');
        return;
      }
      
      // Validar tamaño (máximo 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.errorMessage.set('El archivo no debe superar los 5MB');
        return;
      }
      
      this.selectedFile.set(file);
      
      // Crear preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl.set(e.target?.result as string);
      };
      reader.readAsDataURL(file);
      
      this.errorMessage.set('');
    }
  }

  onSubmit(): void {
    // Validaciones
    if (!this.nombre() || !this.precio() || !this.categoria()) {
      this.errorMessage.set('Por favor, completa todos los campos obligatorios');
      return;
    }

    if (this.precio() <= 0) {
      this.errorMessage.set('El precio debe ser mayor que 0');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const request = this.isEditMode()
      ? this.funkoService.updateFunko(
          this.funkoId()!,
          this.nombre(),
          this.precio(),
          this.categoria(),
          this.selectedFile()
        )
      : this.funkoService.createFunko(
          this.nombre(),
          this.precio(),
          this.categoria(),
          this.selectedFile()
        );

    request.subscribe({
      next: () => {
        this.loading.set(false);
        this.successMessage.set(
          this.isEditMode() ? 'Funko actualizado correctamente' : 'Funko creado correctamente',
        );
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 1500);
      },
      error: (error) => {
        this.loading.set(false);
        this.errorMessage.set(error.error?.message || 'Error al guardar el Funko');
        console.error('Error saving funko:', error);
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/']);
  }

  getImageUrl(imagen: string): string {
    if (!imagen || imagen === 'default.png' || imagen === 'pending-upload.png') {
      return 'https://via.placeholder.com/300x300?text=Sin+Imagen';
    }
    if (imagen.startsWith('http')) {
      return imagen;
    }
    return `https://pruebaanteproyecto.onrender.com/uploads/${imagen}`;
  }
}
