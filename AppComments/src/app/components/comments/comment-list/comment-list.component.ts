import { Component, OnInit } from '@angular/core';
import { Comment } from '../../../models/comment';
import { DomSanitizer } from '@angular/platform-browser';
import {JsonPipe, NgForOf, NgIf} from "@angular/common";
import {CommentService} from "../../../services/comment/comment.service";


@Component({
  selector: 'app-comment-list',
  standalone: true,
  imports: [
    NgForOf,
    JsonPipe,
    NgIf
  ],
  templateUrl: './comment-list.component.html',
  styleUrl: './comment-list.component.css'
})

export class CommentListComponent implements OnInit {
  comments: Comment[] = []; // Всі коментарі
  topLevelComments: Comment[] = []; // Верхньорівневі коментарі
  repliesMap: Map<string, Comment[]> = new Map();
  page: number = 1; // Поточна сторінка
  sortBy: string = 'dateCreated'; // Поле для сортування
  sortOrder: string = 'desc'; // Порядок сортування (LIFO)
  replyingTo: Comment | null = null; // Коментар, на який відповідаємо
  totalComments: number = 0;
  commentsPerPage: number = 25; // Коментарів на сторінку
  totalPages: number = 1;

  constructor(private commentService: CommentService, private sanitizer: DomSanitizer) {}

  ngOnInit() {
    this.loadComments();
  }
  loadComments() {
    this.commentService.getComments(this.page, this.sortBy, this.sortOrder).subscribe({
      next: (data: Comment[]) => {
        console.log('Коментарі з сервера:', data);
        this.comments = data; // Зберігаємо всі коментарі
        this.processComments(); // Викликаємо обробку коментарів
        this.topLevelComments = this.comments.filter(c => !c.parentCommentId); // Фільтруємо верхньорівневі коментарі
        this.totalComments = data.length;
        this.totalPages = Math.ceil(this.totalComments / this.commentsPerPage);
      },
      error: (error) => {
        console.error('Помилка завантаження коментарів:', error);
      }
    });
  }

  sortComments() {
    const order = this.sortOrder === 'desc' ? -1 : 1;

    // Якщо сортуємо по даті
    if (this.sortBy === 'createdAt') {
      this.topLevelComments.sort((a, b) => {
        const dateA = new Date(a.createdAtFormatted).getTime();
        const dateB = new Date(b.createdAtFormatted).getTime();
        return order * (dateA - dateB);
      });
    } else {
      // Сортування по іншому полю (якщо потрібно)
      this.topLevelComments.sort((a, b) => {
        const valueA = a[this.sortBy as keyof Comment] ?? '';
        const valueB = b[this.sortBy as keyof Comment] ?? '';
        return valueA > valueB ? order : valueA < valueB ? -order : 0;
      });
    }
  }


  processComments() {
    console.log("Отримані коментарі:", this.comments);
    this.topLevelComments = this.comments.filter(c => !c.parentCommentId);
    console.log("Кореневі коментарі:", this.topLevelComments);

    // Заповнюємо мапу дочірніх коментарів
    this.comments.forEach(comment => {
      comment.depth = 0;
      if (comment.parentCommentId) {
        // Перевіряємо, чи є вже відповіді на цей коментар в картці
        this.comments.forEach(comment => {
          // Заповнюємо мапу дочірніх коментарів
          if (comment.parentCommentId) {
            if (!this.repliesMap.has(comment.parentCommentId)) {
              this.repliesMap.set(comment.parentCommentId, []);
            }
            this.repliesMap.get(comment.parentCommentId)?.push(comment);
          }
        });

        console.log("Мапа дочірніх коментарів:", this.repliesMap);

        // Визначаємо глибину коментаря
        let depth = 0;
        let parent = this.comments.find(c => c.commentId === comment.parentCommentId);
        while (parent) {
          depth++;
          parent = this.comments.find(c => c.commentId === parent?.parentCommentId);
          if (!parent) break; // Перевірка на випадок, якщо parent не знайдений
          console.log("Глибина коментаря:", depth);
        }
        comment.depth = depth;
      }
    });

    // Оновлюємо `replyCount` для ВСІХ коментарів

    this.comments.forEach(comment => {
      comment.replyCount = this.repliesMap.get(comment.commentId)?.length || 0;
      console.log("comment.id:", comment.commentId);
      console.log("this.repliesMap.get(comment.id)?.length:", this.repliesMap.get(comment.commentId)?.length);
      console.log("replyCount:", comment.replyCount);
    });
    console.log("Мапа дочірніх коментарів:", this.repliesMap);
    this.repliesMap.forEach((replies, parentId) => {
      console.log(`Коментар ${parentId} має ${replies.length} відповідей`);
    });
    this.sortComments();
  }

  replyToComment(comment: Comment) {
    this.replyingTo = comment;
  }

  sanitizeHtml(content: string) {
    return this.sanitizer.bypassSecurityTrustHtml(content);
  }

  onSort(field: string) {
    this.sortBy = field;
    this.sortOrder = this.sortOrder === 'desc' ? 'asc' : 'desc';
    this.sortComments();
  }

  onPageChange(newPage: number) {
    if (newPage > 0 && newPage <= this.totalPages) {
      this.page = newPage;
      this.loadComments();
    }
  }

  getReplies(parentId: string | null): Comment[] {
    if (!parentId) return []; // Якщо parentId null, повертаємо порожній масив
    return this.comments.filter(c => c.parentCommentId === parentId);
  }
  toggleReplies(comment: Comment) {
    console.log('toggleReplies called for comment:', comment); // Додаємо логування
    comment.expanded = !comment.expanded;
    console.log(`Toggled replies for comment ID ${comment.commentId}, expanded:`, comment.expanded); // Логування
  }
}
