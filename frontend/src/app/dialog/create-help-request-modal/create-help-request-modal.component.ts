import { Component, EventEmitter, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HelpRequestService } from '../../services/help-request.service';

@Component({
  selector: 'app-create-help-request-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-help-request-modal.component.html',
  styleUrls: ['./create-help-request-modal.component.scss']
})
export class CreateHelpRequestModalComponent {
  private fb = inject(FormBuilder);
  private helpService = inject(HelpRequestService);

  @Output() close = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();

  createForm = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    content: ['', [Validators.required, Validators.maxLength(1000)]],
    locationId: ['']
  });

  isSubmitting = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result as string;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  removeImage(): void {
    this.selectedFile = null;
    this.imagePreview = null;
  }

  submit(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const { title, content, locationId } = this.createForm.getRawValue();

    this.helpService.createHelpRequest(
      title!,
      content!,
      locationId || undefined,
      this.selectedFile || undefined
    ).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.created.emit();
        this.close.emit();
      },
      error: err => {
        console.error('Помилка створення реквесту:', err);
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.close.emit();
  }
}
