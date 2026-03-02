import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Employee } from '../services/employee.service';

@Component({
  selector: 'app-employee-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="directory-shell">
      <div class="toolbar">
        <div class="filter-group">
          <label>
            Filter
            <input type="text" placeholder="Name or email" [(ngModel)]="filter" (input)="onFilter()" />
          </label>
          <label>
            Org unit
            <input type="text" placeholder="e.g. FIN" [(ngModel)]="orgUnitFilter" (input)="onFilter()" />
          </label>
        </div>
        <button *ngIf="showAddButton" type="button" class="primary" (click)="onAdd()">Add Employee</button>
      </div>

      <div class="status error" *ngIf="error">{{ error }}</div>
      <div class="status" *ngIf="loading">Loading directory…</div>

      <table class="directory-table" *ngIf="!loading && paginatedEmployees.length">
        <thead>
          <tr>
            <th>Name</th>
            <th>Employee ID</th>
            <th>Email</th>
            <th>Department</th>
            <th>Reports To</th>
            <th *ngIf="showAccessActions">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let emp of paginatedEmployees">
            <td>{{ emp.name }}</td>
            <td>{{ emp.id }}</td>
            <td>{{ emp.email }}</td>
            <td>{{ emp.departmentName || emp.orgUnitCode || '—' }}</td>
            <td>{{ emp.reportsToName || (emp.reportsToId || emp.managerId) || '—' }}</td>
            <td *ngIf="showAccessActions" class="actions-cell">
              <label class="status-toggle" [class.is-active]="isUserActive(emp)" [attr.aria-label]="'Toggle active status for ' + emp.name">
                <input
                  type="checkbox"
                  [checked]="isUserActive(emp)"
                  (change)="onToggleStatus(emp, $any($event.target).checked)" />
                <span class="switch-track">
                  <span class="switch-thumb"></span>
                </span>
                <span class="status-text">{{ isUserActive(emp) ? 'Active' : 'Inactive' }}</span>
              </label>
              <button type="button" class="table-action dark" (click)="onAssignRole(emp)">Assign Role</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="status muted" *ngIf="!loading && !paginatedEmployees.length && !error">
        No employees match this filter yet.
      </div>

      <div class="pagination" *ngIf="!loading && totalPages > 1">
        <button type="button" (click)="prevPage()" [disabled]="page === 1">Prev</button>
        <span>Page {{ page }} of {{ totalPages }}</span>
        <button type="button" (click)="nextPage()" [disabled]="page === totalPages">Next</button>
      </div>
    </div>
  `,
  styles: [`
    .directory-shell { display: flex; flex-direction: column; gap: 1.5rem; }
    .toolbar { display: flex; justify-content: space-between; align-items: flex-end; gap: 1rem; flex-wrap: wrap; }
    .filter-group { display: flex; gap: 1rem; flex-wrap: wrap; }
    .filter-group label { font-size: 0.75rem; letter-spacing: 0.2em; text-transform: uppercase; display: flex; flex-direction: column; gap: 0.5rem; color: #555; min-width: 200px; }
    input { border: 1px solid #d0d0d0; border-radius: 999px; padding: 0.65rem 1.2rem; }
    button.primary { border-radius: 999px; padding: 0.75rem 1.6rem; border: 1px solid #050505; background: #050505; color: #fff; letter-spacing: 0.15em; text-transform: uppercase; font-size: 0.75rem; }
    .directory-table { width: 100%; border-collapse: collapse; }
    th, td { text-align: left; padding: 0.85rem 0.5rem; border-bottom: 1px solid #e5e5e5; }
    th { font-size: 0.75rem; letter-spacing: 0.25em; text-transform: uppercase; color: #8a8a8a; }
    .actions-cell { white-space: nowrap; display: flex; gap: 0.45rem; flex-wrap: wrap; }
    .status-toggle { display: inline-flex; align-items: center; gap: 0.45rem; border: 1px solid #d33a2f; border-radius: 999px; padding: 0.28rem 0.7rem; font-size: 0.68rem; letter-spacing: 0.14em; text-transform: uppercase; color: #d33a2f; }
    .status-toggle input { position: absolute; opacity: 0; width: 0; height: 0; }
    .switch-track { width: 2.15rem; height: 1.15rem; border-radius: 999px; background: #d33a2f; border: 1px solid #d33a2f; display: inline-flex; align-items: center; padding: 0.12rem; transition: all 0.2s ease; }
    .switch-thumb { width: 0.78rem; height: 0.78rem; border-radius: 50%; background: #fff; transform: translateX(0); transition: transform 0.2s ease; }
    .status-toggle.is-active { border-color: #1a7f37; color: #1a7f37; }
    .status-toggle.is-active .switch-track { background: #1a7f37; border-color: #1a7f37; }
    .status-toggle.is-active .switch-thumb { transform: translateX(0.96rem); }
    .status-text { min-width: 4.4rem; }
    .table-action { border: 1px solid #111; border-radius: 999px; background: transparent; color: #111; font-size: 0.68rem; letter-spacing: 0.14em; text-transform: uppercase; padding: 0.35rem 0.75rem; }
    .table-action.dark { background: #111; color: #fff; }
    .status { font-size: 0.85rem; letter-spacing: 0.08em; color: #555; }
    .status.error { color: #b4000f; }
    .status.muted { color: #999; text-align: center; padding: 1.5rem 0; }
    .pagination { display: flex; gap: 1rem; align-items: center; justify-content: flex-end; }
    .pagination button { border-radius: 999px; border: 1px solid #111; background: transparent; padding: 0.4rem 1rem; text-transform: uppercase; letter-spacing: 0.2em; font-size: 0.7rem; }
  `]
})
export class EmployeeTableComponent implements OnChanges {
  @Input() employees: Employee[] = [];
  @Input() loading = false;
  @Input() error = '';
  @Input() showAddButton = true;
  @Input() showAccessActions = false;
  @Input() userActiveState: Record<number, boolean> = {};
  @Output() add = new EventEmitter<void>();
  @Output() statusToggle = new EventEmitter<{ employee: Employee; active: boolean }>();
  @Output() assignRole = new EventEmitter<Employee>();

  protected filter = '';
  protected orgUnitFilter = '';
  protected page = 1;
  protected pageSize = 10;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['employees']) {
      this.ensurePageBounds();
    }
  }

  private get filteredEmployees(): Employee[] {
    const term = this.filter.toLowerCase();
    const orgUnitTerm = this.orgUnitFilter.toLowerCase();
    if (!term) {
      return this.employees.filter(emp => this.matchesOrgUnit(emp, orgUnitTerm));
    }
    return this.employees.filter(emp => {
      const matchesText = emp.name.toLowerCase().includes(term) || emp.email.toLowerCase().includes(term);
      return matchesText && this.matchesOrgUnit(emp, orgUnitTerm);
    });
  }

  get paginatedEmployees(): Employee[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filteredEmployees.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    const total = Math.ceil(this.filteredEmployees.length / this.pageSize);
    return total || 1;
  }

  onAdd(): void {
    this.add.emit();
  }

  onToggleStatus(employee: Employee, active: boolean): void {
    this.statusToggle.emit({ employee, active });
  }

  onAssignRole(employee: Employee): void {
    this.assignRole.emit(employee);
  }

  onFilter(): void {
    this.page = 1;
  }

  prevPage(): void {
    if (this.page > 1) {
      this.page--;
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
    }
  }

  private ensurePageBounds(): void {
    const maxPage = Math.max(1, Math.ceil(this.filteredEmployees.length / this.pageSize));
    if (this.page > maxPage) {
      this.page = maxPage;
    }
  }

  private matchesOrgUnit(emp: Employee, orgUnitTerm: string): boolean {
    if (!orgUnitTerm) {
      return true;
    }
    return (emp.orgUnitCode ?? '').toLowerCase().includes(orgUnitTerm);
  }

  protected isUserActive(employee: Employee): boolean {
    const status = this.userActiveState[employee.id];
    return typeof status === 'boolean' ? status : false;
  }
}
