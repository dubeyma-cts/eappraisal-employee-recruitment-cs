import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  message = '';
  loading = false;
  resetLoading = false;
  showReset = false;
  resetEmail = '';
  resetPassword = '';
  resetConfirmPassword = '';

  constructor(private auth: AuthService, private router: Router) {}

  async login() {
    this.loading = true;
    this.error = '';
    this.message = '';
    try {
      const success = await this.auth.login(this.email, this.password);
      if (success) {
        this.router.navigate(['/landing']);
      } else {
        this.error = 'Invalid credentials';
      }
    } catch (e) {
      this.error = 'Login failed';
    }
    this.loading = false;
  }

  toggleReset(): void {
    this.showReset = !this.showReset;
    this.error = '';
    this.message = '';
    this.resetLoading = false;
    if (!this.showReset) {
      this.resetEmail = '';
      this.resetPassword = '';
      this.resetConfirmPassword = '';
    }
  }

  async submitReset(): Promise<void> {
    this.error = '';
    this.message = '';

    const email = this.resetEmail.trim();
    if (!email) {
      this.error = 'Email is required for password reset';
      return;
    }

    if (this.resetPassword.length < 6) {
      this.error = 'New password must be at least 6 characters';
      return;
    }

    if (this.resetPassword !== this.resetConfirmPassword) {
      this.error = 'Passwords do not match';
      return;
    }

    this.resetLoading = true;
    let result: { success: boolean; message: string };
    try {
      result = await this.auth.resetPassword(email, this.resetPassword);
    } catch (e) {
      this.error = 'Password reset failed';
      return;
    } finally {
      this.resetLoading = false;
    }

    if (!result.success) {
      this.error = result.message;
      return;
    }

    this.message = result.message;
    this.email = email;
    this.password = '';
    this.resetPassword = '';
    this.resetConfirmPassword = '';
    this.showReset = false;
  }
}
