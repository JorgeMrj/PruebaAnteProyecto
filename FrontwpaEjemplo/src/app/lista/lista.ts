import { Component, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

interface Funko {
  id: number;
  name: string;
  categoryId: string;
  category?: any;
  imagen: string;
  price: number;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: 'app-lista',
  imports: [CommonModule],
  templateUrl: './lista.html',
  styleUrl: './lista.css',
})
export class Lista implements OnInit {
  private http = inject(HttpClient);
  funkos: Funko[] = [];
  loading: boolean = true;

  ngOnInit(): void {
    this.http.get<Funko[]>('https://pruebaanteproyecto.onrender.com/api/funkos').subscribe({
      next: (data) => {
        this.funkos = data;
        this.loading = false;
      },
    });
  }
}
