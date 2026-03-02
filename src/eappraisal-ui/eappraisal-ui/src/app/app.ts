import { CommonModule } from '@angular/common';
import { Component, DestroyRef, computed, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from './services/auth.service';

type NavItem = {
  label: string;
  route: string;
  queryParams?: { view?: string };
};

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly hrNavItems: NavItem[] = [
    { label: 'Manage Employee', route: '/landing', queryParams: { view: 'employees' } },
    { label: 'Appraisals', route: '/landing', queryParams: { view: 'appraisals' } },
    { label: 'Reports', route: '/landing', queryParams: { view: 'reports' } },
    { label: 'Manage Access', route: '/landing', queryParams: { view: 'access' } }
  ];
  private readonly managerNavItems: NavItem[] = [
    { label: 'Upcoming Appraisals', route: '/landing', queryParams: { view: 'upcoming' } },
    { label: 'Manager Comments', route: '/landing', queryParams: { view: 'comments' } },
    { label: 'Finalize & CTC', route: '/landing', queryParams: { view: 'finalize' } },
    { label: 'Reports', route: '/landing', queryParams: { view: 'reports' } }
  ];
  private readonly employeeNavItems: NavItem[] = [
    { label: 'My Appraisal', route: '/landing', queryParams: { view: 'my-appraisal' } },
    { label: 'Submit Feedback', route: '/landing', queryParams: { view: 'feedback' } },
    { label: 'Reports', route: '/landing', queryParams: { view: 'reports' } }
  ];
  protected readonly navItems = computed(() => {
    if (this.isManagerUser()) {
      return this.managerNavItems;
    }
    if (this.isEmployeeUser()) {
      return this.employeeNavItems;
    }
    if (this.isNoAccessUser()) {
      return [];
    }
    return this.hrNavItems;
  });

  protected readonly userName = signal('Priya Iyer');
  protected readonly userRoleLabel = computed(() => {
    if (this.isManagerUser()) {
      return 'Manager';
    }
    if (this.isEmployeeUser()) {
      return 'Employee';
    }
    if (this.isNoAccessUser()) {
      return 'No Access';
    }
    return 'HR Lead';
  });
  protected readonly isManagerUser = computed(() => this.currentRole() === 'MANAGER');
  protected readonly isEmployeeUser = computed(() => this.currentRole() === 'EMPLOYEE');
  protected readonly isNoAccessUser = computed(() => this.currentRole() === 'NONE');
  private readonly currentRole = signal<'HR' | 'MANAGER' | 'EMPLOYEE' | 'NONE'>('NONE');
  protected readonly userInitials = computed(() => {
    const initials = this.userName()
      .split(' ')
      .filter(part => !!part)
      .map(part => part.charAt(0))
      .join('')
      .slice(0, 2)
      .toUpperCase();
    return initials || 'HR';
  });
  protected readonly breadcrumb = signal('HR / Dashboard');
  protected readonly currentUrl = signal('/');
  protected readonly isAuthRoute = computed(() => {
    const url = this.currentUrl();
    return !url || url === '/' || url.startsWith('/login');
  });

  constructor(
    private readonly router: Router,
    private readonly destroyRef: DestroyRef,
    private readonly auth: AuthService
  ) {
    this.applyUserContext();
    const initialUrl = this.router.url || '/';
    this.currentUrl.set(initialUrl);
    this.breadcrumb.set(this.mapRouteToBreadcrumb(initialUrl));

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(event => {
        this.applyUserContext();
        this.currentUrl.set(event.urlAfterRedirects);
        this.breadcrumb.set(this.mapRouteToBreadcrumb(event.urlAfterRedirects));
      });
  }

  protected onLogout(): void {
    this.router.navigate(['/login']);
  }

  protected isNavActive(item: NavItem): boolean {
    const url = this.currentUrl();
    if (!url.startsWith(item.route)) {
      return false;
    }
    const expectedView = item.queryParams?.view;
    if (!expectedView) {
      return url === item.route || url.startsWith(`${item.route}?`);
    }
    const queryString = url.split('?')[1] ?? '';
    const params = new URLSearchParams(queryString);
    const actualView = params.get('view') ?? this.defaultViewByRole();
    return actualView === expectedView;
  }

  private mapRouteToBreadcrumb(url: string): string {
    const role = this.isManagerUser() ? 'Manager' : this.isEmployeeUser() ? 'Employee' : this.isNoAccessUser() ? 'No Access' : 'HR';
    if (url.startsWith('/landing')) {
      return `${role} / Landing`;
    }
    if (url.startsWith('/login') || url === '/' || url === '') {
      return `${role} / Login`;
    }
    return `${role} / Dashboard`;
  }

  private applyUserContext(): void {
    const username = this.auth.user?.username as string | undefined;
    if (username && username.trim()) {
      this.userName.set(username.trim());
    }

    const roles = this.normalizeRoles(this.auth.user?.roles);
    const hasManagerRole = roles.includes('MANAGER');
    const hasEmployeeRole = roles.includes('EMPLOYEE');
    if (hasManagerRole) {
      this.currentRole.set('MANAGER');
      return;
    }
    if (hasEmployeeRole) {
      this.currentRole.set('EMPLOYEE');
      return;
    }
    const hasHrRole = roles.includes('HR');
    this.currentRole.set(hasHrRole ? 'HR' : 'NONE');
  }

  private defaultViewByRole(): string {
    if (this.isManagerUser()) {
      return 'upcoming';
    }
    if (this.isEmployeeUser()) {
      return 'my-appraisal';
    }
    if (this.isNoAccessUser()) {
      return '';
    }
    return 'employees';
  }

  private normalizeRoles(rawRoles: unknown): string[] {
    if (Array.isArray(rawRoles)) {
      return rawRoles
        .filter((role): role is string => typeof role === 'string')
        .map(role => role.trim().toUpperCase())
        .filter(role => role.length > 0);
    }

    if (typeof rawRoles === 'string') {
      return rawRoles
        .split(',')
        .map(role => role.trim().toUpperCase())
        .filter(role => role.length > 0);
    }

    return [];
  }
}
