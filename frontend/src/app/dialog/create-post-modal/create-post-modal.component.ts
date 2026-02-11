import { Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PostService } from '../../services/post.service';

@Component({
  selector: 'app-create-post-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-post-modal.component.html',
  styleUrls: ['./create-post-modal.component.scss']
})
export class CreatePostModalComponent {
  private fb = inject(FormBuilder);
  private postService = inject(PostService);

  @Output() close = new EventEmitter<void>();
  @Output() postCreated = new EventEmitter<void>(); // Подія успішного створення

  createForm = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    content: ['', [Validators.required, Validators.maxLength(500)]]
  });

  isSubmitting = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  // Обробка вибору файлу
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedFile = file;

      // Створення прев'ю для відображення
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  // Видалення фото
  removeImage(): void {
    this.selectedFile = null;
    this.imagePreview = null;
    // Скидаємо значення input file, якщо він є в DOM (необов'язково для модалки, що закривається)
  }

  submit() {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched(); // Підсвітити помилки, якщо користувач просто натиснув кнопку
      return;
    }

    this.isSubmitting = true;

    // Отримуємо значення. Використовуємо 'as string', бо валідатори гарантують, що там не null
    const { title, content } = this.createForm.getRawValue();

    this.postService.createPost(
      title as string,
      content as string,
      this.selectedFile || undefined
    ).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.postCreated.emit(); // Сигналізуємо батьківському компоненту оновити стрічку
        this.close.emit();       // Закриваємо модалку
      },
      error: (err) => {
        console.error('Помилка створення поста:', err);
        this.isSubmitting = false;
        // Тут можна додати toast notification про помилку
      }
    });
  }

  onCancel() {
    this.close.emit();
  }
}
