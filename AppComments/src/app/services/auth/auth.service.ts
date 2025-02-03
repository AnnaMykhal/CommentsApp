import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import {environment} from "../../../environments/environment";


interface AuthResponse {
  token: string;
  user: any;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/user`;
  private userSubject = new BehaviorSubject<any>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {}

  register(user: { userName: string; email: string; password: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        this.userSubject.next(response.user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('token');
    this.userSubject.next(null);
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('token');
  }

  // getUser(): Observable<any> {
  //   return this.userSubject.asObservable();
  // }
  // getUser(): Observable<any> {
  //   return this.user$; // Повертаємо Observable
  // }
  // getUser(): Observable<any> {
  //   if (!this.userSubject.value) { // Якщо юзер ще не завантажений
  //     this.http.get<any>(`${this.apiUrl}/me`).subscribe(user => this.userSubject.next(user));
  //   }
  //   return this.user$;
  // }
  getUser(): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = { Authorization: `Bearer ${token}` };

    return this.http.get<any>(`${this.apiUrl}/me`, { headers }).pipe(
      tap(user => this.userSubject.next(user))
    );
  }
}
