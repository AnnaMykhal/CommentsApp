import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import {catchError, tap} from 'rxjs/operators';
import { Comment } from '../../models/comment';
import {environment} from "../../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = `${environment.apiUrl}/comment`;

  constructor(private http: HttpClient) {}

  /**
   * Отримує коментарі за сторінками з можливістю сортування.
   * @param page Номер сторінки
   * @param sortBy Поле для сортування
   * @param sortOrder Порядок сортування (asc/desc)
   */
  getComments(page: number, sortBy: string, sortOrder: string): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.apiUrl}?page=${page}&sortBy=${sortBy}&sortOrder=${sortOrder}`)
      .pipe(
        tap(comments => console.log('Отримані коментарі:', comments)),
        catchError(this.handleError)
      );
  }

  /**
   * Додає новий коментар.
   * @param comment Об'єкт коментаря
   */
  addComment(comment: Comment): Observable<any> {
    return this.http.post(this.apiUrl, comment)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Обробка помилок при HTTP запитах.
   * @param error Помилка, що виникла під час запиту
   */
  private handleError(error: any) {
    let errorMessage = 'An unknown error occurred!';
    if (error.error instanceof ErrorEvent) {
      // Серверна або мережна помилка
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Помилка з сервером
      errorMessage = `Server returned code: ${error.status}, error message is: ${error.message}`;
    }
    // Можна також логувати помилки на сервері
    console.error(errorMessage);
    return throwError(errorMessage); // Повертаємо помилку для обробки в компоненті
  }
}
