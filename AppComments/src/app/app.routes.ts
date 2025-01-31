import { Routes } from '@angular/router';
import {RegisterComponent} from "./components/auth/register/register.component";
import { provideRouter } from '@angular/router';
import {LoginComponent} from "./components/auth/login/login.component";
import {HomeComponent} from "./components/home/home.component";


export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: '**', redirectTo: '' }
];
export const appRouting = provideRouter(routes);
