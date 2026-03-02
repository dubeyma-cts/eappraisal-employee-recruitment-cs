import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthService {
  user: any = null;
  private apiUrl = 'http://localhost:8080/api/v1/identity/auth/login'; // Updated to match backend

  constructor(private http: HttpClient) {}

  async login(username: string, password: string): Promise<boolean> {
    try {
      // Send as form data (application/x-www-form-urlencoded)
      const body = new URLSearchParams();
      body.set('username', username);
      body.set('password', password);
      const res: any = await this.http.post(this.apiUrl, body.toString(), {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
      }).toPromise();
      if (res && res.status === 'success') {
        // Optionally store user info or roles
        this.user = {
          username,
          roles: res.roles
        };
        return true;
      }
      return false;
    } catch {
      return false;
    }
  }

  logout() {
    localStorage.removeItem('token');
    this.user = null;
  }
}
