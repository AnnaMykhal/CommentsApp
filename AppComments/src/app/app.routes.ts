import { Routes } from '@angular/router';
import {RegisterComponent} from "./components/auth/register/register.component";
import { provideRouter } from '@angular/router';
import {LoginComponent} from "./components/auth/login/login.component";
import {HomeComponent} from "./components/home/home.component";
import {CommentListComponent} from "./components/comments/comment-list/comment-list.component";
import {CommentFormComponent} from "./components/comments/comment-form/comment-form.component";



export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'new-comment', component: CommentFormComponent},
  { path: 'comments', component: CommentListComponent},
  { path: '**', redirectTo: '' }
];
export const appRouting = provideRouter(routes);
