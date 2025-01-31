import { Component, OnInit } from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {CommentService} from "../../../services/comment/comment.service";




@Component({
  selector: 'app-comment-form',
  standalone: true,
  imports: [
    ReactiveFormsModule
  ],
  templateUrl: './comment-form.component.html',
  styleUrl: './comment-form.component.css'
})

export class CommentFormComponent {
  commentForm: FormGroup;

  constructor(private fb: FormBuilder, private commentService: CommentService) {
    this.commentForm = this.fb.group({
      userName: ['', [Validators.required, Validators.pattern('^[a-zA-Z0-9]+$')]],  // Латинські букви та цифри
      email: ['', [Validators.required, Validators.email]],  // Формат email
      homePage: ['', [Validators.pattern('https?://.*')]],  // Формат URL
      captcha: ['', [Validators.required]],  // Обов'язкове поле
      text: ['', [Validators.required, Validators.maxLength(500)]],  // Текст коментаря
    });
  }

  onSubmit() {
    if (this.commentForm.valid) {
      const newComment = this.commentForm.value;
      this.commentService.addComment(newComment).subscribe(response => {
        console.log('Comment added:', response);
      });
    }
  }
}
