import { Component, OnInit } from '@angular/core';
import { Comment } from '../../../models/comment';
import { DomSanitizer } from '@angular/platform-browser';
import {NgForOf} from "@angular/common";
import {CommentService} from "../../../services/comment/comment.service";

@Component({
  selector: 'app-comment-list',
  standalone: true,
  imports: [
    NgForOf
  ],
  templateUrl: './comment-list.component.html',
  styleUrl: './comment-list.component.css'
})
export class CommentListComponent implements OnInit {
  comments: Comment[] = [];  // Список коментарів
  page: number = 1;  // Номер поточної сторінки
  sortBy: string = 'dateCreated';  // Поле для сортування
  sortOrder: string = 'desc';  // Порядок сортування (спадний)

  constructor(private commentService: CommentService, private sanitizer: DomSanitizer) {}

  ngOnInit() {
    this.loadComments();  // Завантажуємо коментарі при ініціалізації компонента
  }

  // Завантаження коментарів з API
  loadComments() {
    this.commentService.getComments(this.page, this.sortBy, this.sortOrder).subscribe(comments => {
      this.comments = comments;  // Оновлюємо список коментарів
    });
  }

  // Очищення HTML за допомогою DomSanitizer
  sanitizeHtml(content: string) {
    return this.sanitizer.bypassSecurityTrustHtml(content);
  }

  // Обробка сортування по полях
  onSort(field: string) {
    this.sortBy = field;  // Вибір поля для сортування
    this.sortOrder = this.sortOrder === 'desc' ? 'asc' : 'desc';  // Перемикання порядку сортування
    this.loadComments();  // Перезавантажуємо коментарі
  }

  // Обробка зміни сторінки
  onPageChange(page: number) {
    this.page = page;  // Оновлюємо номер сторінки
    this.loadComments();  // Завантажуємо коментарі для нової сторінки
  }
}
