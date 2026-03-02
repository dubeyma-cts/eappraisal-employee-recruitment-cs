import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppraisalCandidate } from '../services/employee.service';

@Component({
  selector: 'app-appraisal-assignment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="assignment-shell">
      <div class="toolbar">
        <label>
          Search employee
          <input [(ngModel)]="searchTerm" (input)="applyFilter()" placeholder="Name or email" />
        </label>
      </div>

      <div class="status" *ngIf="loading">Loading upcoming appraisals...</div>
      <div class="status error" *ngIf="error">{{ error }}</div>

      <div class="board" *ngIf="!loading">
        <section>
          <h3>Eligible <span>{{ eligible.length }}</span></h3>
          <table *ngIf="eligible.length; else noEligible">
            <thead>
              <tr>
                <th>Employee</th>
                <th>Department</th>
                <th>Reports To</th>
                <th>Due Date</th>
                <th *ngIf="canAssign"></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of eligible">
                <td>
                  <strong>{{ item.employeeName }}</strong>
                  <small>{{ item.employeeEmail }}</small>
                </td>
                <td>{{ item.departmentName || '—' }}</td>
                <td>{{ item.assignedManagerName || item.reportsToName || '—' }}</td>
                <td>{{ item.dueDate | date: 'dd-MMM-yyyy' }}</td>
                <td *ngIf="canAssign">
                  <button type="button" (click)="onAssign(item, $event)">Assign</button>
                </td>
              </tr>
            </tbody>
          </table>
          <ng-template #noEligible><p class="empty">No eligible employees.</p></ng-template>
        </section>

        <section>
          <h3>Blocked <span>{{ blocked.length }}</span></h3>
          <table *ngIf="blocked.length; else noBlocked">
            <thead>
              <tr>
                <th>Employee</th>
                <th>Blocker</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of blocked">
                <td>
                  <strong>{{ item.employeeName }}</strong>
                  <small>{{ item.employeeEmail }}</small>
                </td>
                <td>{{ item.blockerReason || 'Missing manager mapping' }}</td>
              </tr>
            </tbody>
          </table>
          <ng-template #noBlocked><p class="empty">No blocked employees.</p></ng-template>
        </section>

        <section>
          <h3>Assigned <span>{{ assigned.length }}</span></h3>
          <table *ngIf="assigned.length; else noAssigned">
            <thead>
              <tr>
                <th>Employee</th>
                <th>Assigned Manager</th>
                <th>Status</th>
                <th *ngIf="enableDetails">View</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of assigned">
                <td>
                  <strong>{{ item.employeeName }}</strong>
                  <small>{{ item.employeeEmail }}</small>
                </td>
                <td>{{ item.assignedManagerName || '—' }}</td>
                <td>{{ item.assignmentStatus }}</td>
                <td *ngIf="enableDetails">
                  <button type="button" class="view-icon" (click)="onView(item, $event)" aria-label="View employee details">👁</button>
                </td>
              </tr>
            </tbody>
          </table>
          <ng-template #noAssigned><p class="empty">No assigned cases yet.</p></ng-template>
        </section>
      </div>
    </section>
  `,
  styles: [`
    .assignment-shell { display: flex; flex-direction: column; gap: 1rem; }
    .toolbar label { display: inline-flex; flex-direction: column; gap: 0.4rem; text-transform: uppercase; font-size: 0.72rem; letter-spacing: 0.2em; }
    input { border: 1px solid #d0d0d0; border-radius: 999px; padding: 0.6rem 1rem; min-width: 260px; }
    .board { display: grid; grid-template-columns: 1fr; gap: 1rem; }
    section { border: 1px solid #ececec; border-radius: 1rem; padding: 1rem; background: #fff; }
    h3 { margin: 0 0 0.9rem; font-size: 0.95rem; text-transform: uppercase; letter-spacing: 0.15em; display: flex; justify-content: space-between; }
    table { width: 100%; border-collapse: collapse; }
    th, td { text-align: left; border-bottom: 1px solid #ececec; padding: 0.65rem 0.4rem; font-size: 0.9rem; }
    td small { display: block; color: #777; margin-top: 0.2rem; }
    button { border: 1px solid #050505; background: #050505; color: #fff; border-radius: 999px; padding: 0.35rem 0.9rem; font-size: 0.72rem; text-transform: uppercase; letter-spacing: 0.13em; }
    .view-icon { width: 2rem; height: 2rem; padding: 0; border-radius: 50%; font-size: 1rem; line-height: 1; text-transform: none; letter-spacing: normal; }
    .status { color: #555; font-size: 0.9rem; }
    .status.error { color: #a80016; }
    .empty { color: #8c8c8c; margin: 0.4rem 0; }
  `]
})
export class AppraisalAssignmentComponent {
  @Input() candidates: AppraisalCandidate[] = [];
  @Input() loading = false;
  @Input() error = '';
  @Input() canAssign = true;
  @Input() enableDetails = false;
  @Output() assign = new EventEmitter<AppraisalCandidate>();
  @Output() viewDetails = new EventEmitter<AppraisalCandidate>();

  protected searchTerm = '';

  protected get eligible(): AppraisalCandidate[] {
    return this.filteredCandidates().filter(item => item.category === 'ELIGIBLE');
  }

  protected get blocked(): AppraisalCandidate[] {
    return this.filteredCandidates().filter(item => item.category === 'BLOCKED');
  }

  protected get assigned(): AppraisalCandidate[] {
    return this.filteredCandidates().filter(item => item.category === 'ASSIGNED');
  }

  protected applyFilter(): void {
    this.searchTerm = this.searchTerm.trimStart();
  }

  private filteredCandidates(): AppraisalCandidate[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) {
      return this.candidates;
    }

    return this.candidates.filter(item =>
      item.employeeName.toLowerCase().includes(term) || item.employeeEmail.toLowerCase().includes(term)
    );
  }

  protected onAssign(candidate: AppraisalCandidate, event: Event): void {
    event.stopPropagation();
    this.assign.emit(candidate);
  }

  protected onView(candidate: AppraisalCandidate, event: Event): void {
    event.stopPropagation();
    this.viewDetails.emit(candidate);
  }
}
