import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DepartmentOption, Employee, EmployeeService, ManagerOption } from '../services/employee.service';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [FormsModule, CommonModule],
  template: `
    <form class="employee-form" *ngIf="employee" (ngSubmit)="onSubmit()">
      <div class="form-grid">
        <label>
          Name
          <input [(ngModel)]="employee.name" name="name" placeholder="Alex Johnson" required />
        </label>

        <label>
          Address
          <input [(ngModel)]="employee.address" name="address" placeholder="12 Park Street" />
        </label>

        <label>
          City
          <input [(ngModel)]="employee.city" name="city" placeholder="Bengaluru" />
        </label>

        <label>
          Phone
          <input [(ngModel)]="employee.phone" name="phone" placeholder="08012345678" />
        </label>

        <label>
          Mobile
          <input [(ngModel)]="employee.mobile" name="mobile" placeholder="9876543210" />
        </label>

        <label>
          Email
          <input type="email" [(ngModel)]="employee.email" name="email" placeholder="alex@company.com" required />
        </label>

        <label>
          DOB
          <input type="date" [(ngModel)]="employee.dob" name="dob" />
        </label>

        <label>
          Gender
          <select [(ngModel)]="employee.gender" name="gender">
            <option value="">Select gender</option>
            <option *ngFor="let option of genderOptions" [value]="option">{{ option }}</option>
          </select>
        </label>

        <label>
          Marital Status
          <select [(ngModel)]="employee.maritalStatus" name="maritalStatus">
            <option value="">Select status</option>
            <option *ngFor="let option of maritalOptions" [value]="option">{{ option }}</option>
          </select>
        </label>

        <label>
          DOJ
          <input type="date" [(ngModel)]="employee.doj" name="doj" />
        </label>

        <label>
          Passport
          <input [(ngModel)]="employee.passport" name="passport" placeholder="M1234567" />
        </label>

        <label>
          PAN
          <input [(ngModel)]="employee.pan" name="pan" placeholder="ABCDE1234F" />
        </label>

        <label>
          Work Experience (Years)
          <input type="number" min="0" [(ngModel)]="employee.workExperience" name="workExperience" placeholder="6" />
        </label>

        <label class="reports-to">
          Reports To
          <input
            [(ngModel)]="reportsToQuery"
            name="reportsTo"
            placeholder="Search manager by name"
            (input)="onReportsToSearch()"
            autocomplete="off"
          />
          <div class="suggestions" *ngIf="managerSuggestions.length">
            <button type="button" *ngFor="let manager of managerSuggestions" (click)="selectManager(manager)">
              {{ manager.displayLabel }}
            </button>
          </div>
        </label>

        <label>
          Department
          <select [(ngModel)]="employee.departmentId" name="departmentId" required (change)="onDepartmentChange()">
            <option [ngValue]="null">Select department</option>
            <option *ngFor="let dept of departments" [ngValue]="dept.id">{{ dept.name }}</option>
          </select>
          <small *ngIf="loadingDepartments">Loading departments...</small>
        </label>
      </div>

      <div class="form-actions">
        <button class="primary" type="submit">Save employee</button>
        <button class="ghost" type="button" (click)="onCancel()">Cancel</button>
      </div>
    </form>
  `,
  styles: [`
    .employee-form { display: flex; flex-direction: column; gap: 1.5rem; }
    .form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 1rem; }
    label { display: flex; flex-direction: column; gap: 0.5rem; text-transform: uppercase; font-size: 0.75rem; letter-spacing: 0.2em; color: #4a4a4a; }
    input, select { border: 1px solid #d0d0d0; border-radius: 0.9rem; padding: 0.85rem 1rem; font-size: 0.95rem; background: #fafafa; }
    small { color: #666; font-size: 0.72rem; letter-spacing: 0.08em; }
    .reports-to { position: relative; }
    .suggestions { position: absolute; top: calc(100% + 0.35rem); left: 0; right: 0; background: #fff; border: 1px solid #d0d0d0; border-radius: 0.8rem; box-shadow: 0 10px 24px rgba(0,0,0,0.12); z-index: 10; max-height: 200px; overflow-y: auto; }
    .suggestions button { width: 100%; text-align: left; border: none; background: #fff; padding: 0.65rem 0.85rem; font-size: 0.85rem; letter-spacing: 0.04em; }
    .suggestions button:hover { background: #f5f5f5; }
    .form-actions { display: flex; gap: 0.75rem; }
    .form-actions button { border-radius: 999px; padding: 0.85rem 1.6rem; text-transform: uppercase; letter-spacing: 0.15em; font-size: 0.75rem; }
    .primary { background: #050505; color: #fff; border: 1px solid #050505; }
    .ghost { background: transparent; color: #050505; border: 1px solid rgba(0, 0, 0, 0.3); }
  `]
})
export class EmployeeProfileFormComponent implements OnInit {
  @Input() employee: Employee | null = null;
  @Output() save = new EventEmitter<Employee>();
  @Output() cancel = new EventEmitter<void>();

  protected readonly genderOptions = ['Male', 'Female', 'Other', 'Prefer not to say'];
  protected readonly maritalOptions = ['Single', 'Married', 'Divorced', 'Widowed'];
  protected departments: DepartmentOption[] = [];
  protected managerSuggestions: ManagerOption[] = [];
  protected reportsToQuery = '';
  protected loadingDepartments = false;

  constructor(private readonly employeeService: EmployeeService) {}

  ngOnInit(): void {
    this.loadingDepartments = true;
    this.employeeService.getDepartments().subscribe({
      next: departments => {
        this.departments = departments;
        this.loadingDepartments = false;
      },
      error: () => {
        this.departments = [];
        this.loadingDepartments = false;
      }
    });

    if (this.employee?.reportsToId && this.employee?.reportsToName) {
      this.reportsToQuery = `${this.employee.reportsToName} (${this.employee.reportsToId})`;
    }
  }

  onSubmit(): void {
    if (!this.employee) {
      return;
    }

    if (this.employee.workExperience !== null && this.employee.workExperience !== undefined) {
      const years = Number(this.employee.workExperience);
      this.employee.workExperience = Number.isNaN(years) ? null : years;
    }

    this.employee.managerId = this.employee.reportsToId ?? null;
    this.save.emit({ ...this.employee });
  }

  onReportsToSearch(): void {
    if (!this.employee) {
      return;
    }
    const term = this.reportsToQuery.trim();
    if (!term) {
      this.employee.reportsToId = null;
      this.employee.reportsToName = '';
      this.employee.managerId = null;
      this.managerSuggestions = [];
      return;
    }

    this.employeeService.searchManagers(term).subscribe({
      next: items => {
        this.managerSuggestions = items;
      },
      error: () => {
        this.managerSuggestions = [];
      }
    });
  }

  selectManager(option: ManagerOption): void {
    if (!this.employee) {
      return;
    }
    this.employee.reportsToId = option.id;
    this.employee.reportsToName = option.name;
    this.employee.managerId = option.id;
    this.reportsToQuery = option.displayLabel;
    this.managerSuggestions = [];
  }

  onDepartmentChange(): void {
    if (!this.employee) {
      return;
    }
    const selected = this.departments.find(department => department.id === this.employee?.departmentId);
    this.employee.departmentName = selected?.name ?? '';
    this.employee.orgUnitCode = selected?.name ?? '';
  }

  onCancel(): void {
    this.cancel.emit();
  }
}
