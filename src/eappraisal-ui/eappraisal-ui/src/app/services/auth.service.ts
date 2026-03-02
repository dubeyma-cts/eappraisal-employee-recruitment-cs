import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  user: any = null;
  private apiUrl = `${environment.api.identityBaseUrl}/api/v1/identity/auth/login`;
  private resetPasswordUrl = `${environment.api.identityBaseUrl}/api/v1/identity/auth/reset-password`;

  constructor(private http: HttpClient) {}

  async login(username: string, password: string): Promise<boolean> {
    try {
      // Send as form data (application/x-www-form-urlencoded)
      const body = new URLSearchParams();
      body.set('username', username);
      body.set('password', password);
      console.log('Sending login request:', body.toString());
      const res: any = await this.http.post(this.apiUrl, body.toString(), {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
      }).toPromise();
      console.log('Login response:', res);
      if (res && res.status === 'success') {
        const userId = Number(res.userId);
        this.user = {
          username,
          roles: res.roles,
          userId: Number.isFinite(userId) ? userId : null
        };
        return true;
      }
      return false;
    } catch (err) {
      console.error('Login error:', err);
      return false;
    }
  }

  async resetPassword(username: string, newPassword: string): Promise<{ success: boolean; message: string }> {
    try {
      const body = new URLSearchParams();
      body.set('username', username);
      body.set('newPassword', newPassword);

      const res: any = await this.http.post(this.resetPasswordUrl, body.toString(), {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
      }).toPromise();

      if (res?.status === 'success') {
        return { success: true, message: res?.message || 'Password reset successful' };
      }

      return { success: false, message: res?.message || 'Password reset failed' };
    } catch (err: any) {
      const message = err?.error?.message || 'Password reset failed';
      return { success: false, message };
    }
  }

  logout() {
    localStorage.removeItem('token');
    this.user = null;
  }
}
