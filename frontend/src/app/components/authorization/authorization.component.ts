import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgIf } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, NgIf],
  templateUrl: './authorization.component.html',
  styleUrls: ['./authorization.component.scss']
})
export class AuthorizationComponent {
  public errorMessage: string | null = null;
  public isRegisterMode = false;

  public authForm = new FormGroup({
    username: new FormControl('', [Validators.required]),
    password: new FormControl('', [Validators.required, Validators.minLength(6)]),

    name: new FormControl('', []),
    fullName: new FormControl('', []),

    role: new FormControl('', { nonNullable: true })
  });


  constructor(private authService: AuthService, private router: Router) {
    this.toggleFormControls();
  }

  toggleMode() {
    this.isRegisterMode = !this.isRegisterMode;
    this.errorMessage = null;
    this.toggleFormControls();
  }

  toggleFormControls() {
    const roleControl = this.authForm.get('role');
    const nameControl = this.authForm.get('name');
    const fullNameControl = this.authForm.get('fullName');

    if (this.isRegisterMode) {
      roleControl?.setValidators([Validators.required]);

      nameControl?.setValidators([Validators.required, Validators.minLength(2)]);
      fullNameControl?.setValidators([Validators.required, Validators.minLength(2)]);

      roleControl?.setValue('');
      nameControl?.setValue('');
      fullNameControl?.setValue('');
    } else {
      roleControl?.clearValidators();

      nameControl?.clearValidators();
      fullNameControl?.clearValidators();

      roleControl?.setValue('');
      nameControl?.setValue('');
      fullNameControl?.setValue('');
    }

    roleControl?.updateValueAndValidity();
    nameControl?.updateValueAndValidity();
    fullNameControl?.updateValueAndValidity();
  }


  onSubmit() {
    if (this.authForm.invalid) {
      this.errorMessage = 'Заповніть усі поля правильно!';
      return;
    }

    const { username, password, role, name, fullName } = this.authForm.value;

    if (this.isRegisterMode) {
      this.authService.register(username!, password!, role!, name!, fullName!).subscribe({
        next: () => {
          this.errorMessage = null;
          this.isRegisterMode = false;
          this.toggleFormControls(); // ✅ повернути форму в режим логіну
          this.checkSessionAndRedirect();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = err.error?.message || 'Не вдалося зареєструватися';
        }
      });
    } else {
      this.authService.login(username!, password!).subscribe({
        next: () => {
          this.errorMessage = null;
          this.checkSessionAndRedirect();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = err.error?.message || 'Невірний логін або пароль';
        }
      });
    }
  }


  private checkSessionAndRedirect() {
    this.authService.checkSession().subscribe({
      next: (isValid: boolean) => {
        if (isValid) {
          this.router.navigate(['/home']);
        } else {
          this.errorMessage = 'Сесія невалідна після авторизації';
        }
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.message || 'Не вдалося перевірити сесію';
      }
    });
  }
  hidePassword = true; // Додай цю змінну

  togglePasswordVisibility() {
    this.hidePassword = !this.hidePassword;
  }
}
