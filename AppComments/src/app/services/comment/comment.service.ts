import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {Observable} from "rxjs";
import {Comment} from "../../models/comment";


@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private apiUrl = 'http://5017/api/comments';

  constructor(private http: HttpClient) {}

  // getComments(page: number): Observable<Comment[]> {
  //   return this.http.get<Comment[]>(`${this.apiUrl}?page=${page}`);
  // }
  getComments(page: number, sortBy: string, sortOrder: string): Observable<Comment[]> {
    // Підключаємо параметри до запиту
    return this.http.get<Comment[]>(`${this.apiUrl}?page=${page}&sortBy=${sortBy}&sortOrder=${sortOrder}`);
  }

  addComment(comment: Comment): Observable<any> {
    return this.http.post(this.apiUrl, comment);
  }
}
