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

  // Увімкнути або вимкнути поле "role"
  toggleFormControls() {
    const roleControl = this.authForm.get('role');

    if (this.isRegisterMode) {
      roleControl?.setValidators([Validators.required]);
      roleControl?.setValue('');
    } else {
      roleControl?.clearValidators();
      roleControl?.setValue('');
    }

    roleControl?.updateValueAndValidity();
  }

  onSubmit() {
    if (this.authForm.invalid) {
      this.errorMessage = 'Заповніть усі поля правильно!';
      return;
    }

    const { username, password, role } = this.authForm.value;

    if (this.isRegisterMode) {
      this.authService.register(username!, password!, role!).subscribe({
        next: () => {
          console.log('Реєстрація успішна');
          this.errorMessage = null;
          this.isRegisterMode = false;
          this.checkSessionAndRedirect();
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage = err.error?.message || 'Не вдалося зареєструватися';
        }
      });
    } else {
      this.authService.login(username!, password!).subscribe({
        next: () => {
          console.log('Вхід успішний');
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
}
