import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';
  loading = false;

  constructor(private auth: AuthService, private router: Router) {}

  async login() {
    this.loading = true;
    this.error = '';
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
}
