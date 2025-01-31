import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import {AuthService} from "./services/auth/auth.service";



@Component({
  selector: 'app-root',
  standalone: true,
 imports: [RouterOutlet, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  user: any;

  constructor(public auth: AuthService) {
    this.auth.getUser().subscribe(user => this.user = user);
  }

  ngOnInit() {
    this.auth.getUser().subscribe(user => {
      this.user = user;
    });
  }

  logout() {
    this.auth.logout();
  }
}






