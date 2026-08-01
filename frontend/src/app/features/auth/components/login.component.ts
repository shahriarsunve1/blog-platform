import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./auth-form.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule, RouterModule]
})
export class LoginComponent implements OnInit {
  form!: FormGroup;
  isLoading = false;
  errorMessage = '';
  showResendVerification = false;
  isResending = false;
  resendSent = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.showResendVerification = false;
    this.resendSent = false;

    this.authService.login(this.form.value).subscribe({
      next: () => {
        this.router.navigate(['/posts']);
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Login failed';
        this.showResendVerification = this.errorMessage.toLowerCase().includes('verify your email');
        this.isLoading = false;
      }
    });
  }

  resendVerification(): void {
    const email = this.form.value.email;
    if (!email) return;

    this.isResending = true;
    this.authService.resendVerification(email).subscribe({
      next: () => {
        this.isResending = false;
        this.resendSent = true;
      },
      error: () => {
        this.isResending = false;
        this.resendSent = true;
      }
    });
  }
}
