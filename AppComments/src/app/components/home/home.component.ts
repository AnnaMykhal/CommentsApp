import {Component, OnInit} from '@angular/core';
import {AuthService} from "../../services/auth/auth.service";
import {NgIf} from "@angular/common";
import {RouterLink} from "@angular/router";

@Component({
  selector: 'app-home',
  template: `
    <div class="home-container">
      <h1>Welcome to the CommentHub!</h1>
      <p>We are glad to see you here! Check out our features and share your thoughts with other users..</p>

      <div *ngIf="user; else guest">
        <h2>Hello, {{ user.userName }}!</h2>
        <p>Welcome back to our app.</p>
        <button (click)="createNewComment()">Create New Comment</button>
      </div>

      <ng-template #guest>
        <h2 class="guest-user">Guest User</h2>
        <p>You can also share your thoughts as a guest. No need to log in!</p>
        <button class="guest-comment-btn" routerLink="/new-comment">Create new comment as Guest</button>
        <p>Want to save your comments and interact with others? <a routerLink="/register">Register</a> or <a routerLink="/login">Log in</a>!</p>
        <p>See all comments <a routerLink="/comments">here</a>.</p>
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
        console.log('User data:', user);
        this.user = user;  // Призначаємо отриманого користувача
      },
      error: (err) => {
        console.error('Error fetching user:', err);
        this.user = null;  // Якщо сталася помилка, забезпечимо значення null
      }
    });
  }

  createNewComment() {
    console.log('New comment form opened');
  }
}
