import { Routes } from '@angular/router';
import { Lista } from './lista/lista';
import { Login } from './login/login';
import { FunkoForm } from './funko-form/funko-form';

export const routes: Routes = [
  { path: '', component: Lista },
  { path: 'lista', component: Lista },
  { path: 'login', component: Login },
  { path: 'funko/new', component: FunkoForm },
  { path: 'funko/edit/:id', component: FunkoForm },
];
