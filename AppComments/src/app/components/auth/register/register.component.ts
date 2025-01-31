import { Component } from '@angular/core';
import { Router } from '@angular/router';
import {FormsModule} from "@angular/forms";
import {AuthService} from "../../../services/auth/auth.service";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    FormsModule,
    NgIf
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  user = { Username: '', Email: '', Password: '' };
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  register() {
    const userToRegister = {
      userName: this.user.Username,
      email: this.user.Email,
      password: this.user.Password
    };

    this.authService.register(userToRegister).subscribe({
      next: () => this.router.navigate(['/login']),
      error: (err: { error: { message: string; }; }) => this.errorMessage = err.error.message
    });
  }
}
