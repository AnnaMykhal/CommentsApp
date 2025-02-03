import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import { CommentService } from '../../../services/comment/comment.service';
import { Comment } from '../../../models/comment';
import {NgIf, NgFor, CommonModule, NgOptimizedImage} from '@angular/common';


@Component({
  selector: 'app-comment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgIf, NgFor, NgOptimizedImage, FormsModule],
  templateUrl: './comment-form.component.html',
  styleUrl: './comment-form.component.css'
})
export class CommentFormComponent implements OnInit {
  commentForm: FormGroup;
  previewText: string = '';
  captchaCode: string = '';
  userCaptchaInput: string = '';
  fileError: string | null = null;
  @Input() parentComment: any | null = null;
  @Output() commentAdded = new EventEmitter<Comment>();

  constructor(private fb: FormBuilder, private commentService: CommentService) {
    this.commentForm = this.fb.group({
      userName: ['', [Validators.required, Validators.pattern('^[A-Za-z0-9]+$')], { updateOn: 'blur' }],
      email: ['', [Validators.required, Validators.email], { updateOn: 'blur' }],
      homePage: ['', Validators.pattern(/^(https?:\/\/)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*\/?$/)],
      text: ['', Validators.required],
      captcha: ['', Validators.required],
      file: [null]
    });

  }

  ngOnInit(): void {
    this.generateCaptcha();
  }

  addTag(tag: string) {
    const text = this.commentForm.get('text')?.value;
    this.commentForm.patchValue({ text: text + `<${tag}></${tag}>` });
  }

  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const validTypes = ['image/jpeg', 'image/png', 'image/gif', 'text/plain'];
      if (!validTypes.includes(file.type)) {
        this.fileError = 'Invalid file type. Allowed: JPG, PNG, GIF, TXT.';
        return;
      } else {
        this.fileError = null;
      }
      if (file.type.startsWith('image/')) {
        this.resizeImage(file);
      } else if (file.type === 'text/plain' && file.size <= 100000) {
        this.commentForm.patchValue({ file });
      } else {
        this.fileError = 'File too large.';
      }
    }
  }

  resizeImage(file: File) {
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = (event: any) => {
      const img = new Image();
      img.src = event.target.result;
      img.onload = () => {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        const maxWidth = 320, maxHeight = 240;
        let width = img.width, height = img.height;

        if (width > maxWidth || height > maxHeight) {
          const ratio = Math.min(maxWidth / width, maxHeight / height);
          width *= ratio;
          height *= ratio;
        }

        canvas.width = width;
        canvas.height = height;
        ctx?.drawImage(img, 0, 0, width, height);

        canvas.toBlob(blob => {
          this.commentForm.patchValue({ file: blob });
        }, file.type);
      };
    };
  }

  previewComment() {
    this.previewText = this.commentForm.get('text')?.value;
  }

  generateCaptcha(): void {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789';
    this.captchaCode = Array.from({ length: 6 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
  }

  validateCaptcha(): boolean {
    return this.userCaptchaInput === this.captchaCode;
  }

  submitComment() {
    if (!this.validateCaptcha()) {
      alert('CAPTCHA введена неправильно! Спробуйте ще раз.');
      return;
    }
    if (this.commentForm.valid) {
      const newComment: Comment = {
        commentId: "",
        username: this.commentForm.value.userName,
        email: this.commentForm.value.email,
        homePage: this.commentForm.value.homePage || null,
        captcha: this.commentForm.value.captcha,
        content: this.commentForm.value.text,
        parentCommentId: this.commentForm.value.parentId || null,
        createdAtFormatted: new Date().toISOString(), // Поточна дата
        replies: [], // Порожній масив для відповідей
        replyCount: 0,
      };

      this.commentService.addComment(newComment).subscribe(
        (response) => {
          console.log('Коментар успішно додано:', response);
          this.commentForm.reset(); // Очистити форму
          this.generateCaptcha();
          this.commentAdded.emit(response); // Оновити список коментарів (якщо використовуєте Output)
        },
        (error) => {
          console.error('Помилка під час додавання коментаря:', error);
        }
      );
    } else {
      console.log('Форма недійсна');
    }
  }
  insertTag(tag: string): void {
    const textControl = this.commentForm.get('text');
    if (textControl) {
      const currentText = textControl.value;
      const newText = currentText + `<${tag}></${tag}>`;
      textControl.setValue(newText);
    }
  }

}
