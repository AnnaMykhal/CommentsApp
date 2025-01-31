import {Component, OnInit} from '@angular/core';
import {AuthService} from "../../services/auth/auth.service";
import {NgIf} from "@angular/common";
import {RouterLink} from "@angular/router";

@Component({
  selector: 'app-home',
  template: `
    <div class="home-container">
      <h1>Welcome to the Home Page!</h1>
      <p>This is the landing page of the application.</p>

      <div *ngIf="user; else guest">
        <h2>Hello, {{ user.username }}!</h2>
        <p>Welcome back to our app.</p>
      </div>

      <ng-template #guest>
        <h2>Guest User</h2>
        <p>Please <a routerLink="/login">log in</a> to continue.</p>
      </ng-template>
    </div>`,
  standalone: true,
  imports: [
    NgIf,
    RouterLink
  ],
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  user: any = null; // Для збереження користувача

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.authService.getUser().subscribe({
      next: (user) => {
        this.user = user;  // Призначаємо отриманого користувача
      },
      error: (err) => {
        console.error('Error fetching user:', err);
        this.user = null;  // Якщо сталася помилка, забезпечимо значення null
      }
    });
  }
}
