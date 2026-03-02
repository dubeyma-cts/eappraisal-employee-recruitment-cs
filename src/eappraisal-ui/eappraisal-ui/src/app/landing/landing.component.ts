import { CommonModule } from '@angular/common';
import { Component, DestroyRef, computed, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AccessRole, AppraisalCandidate, CtcSnapshotView, Employee, EmployeeService, FeedbackComment, ReportRow } from '../services/employee.service';
import { AppraisalAssignmentComponent } from './appraisal-assignment.component';
import { EmployeeProfileFormComponent } from './employee-profile-form.component';
import { EmployeeTableComponent } from './employee-table.component';

type LandingSection = 'employees' | 'appraisals' | 'reports' | 'access' | 'upcoming' | 'comments' | 'finalize' | 'my-appraisal' | 'feedback';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, FormsModule, EmployeeProfileFormComponent, EmployeeTableComponent, AppraisalAssignmentComponent, RouterLink],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.scss']
})
export class LandingComponent {
  protected readonly isHrUser: boolean;
  protected readonly isManagerUser: boolean;
  protected readonly isEmployeeUser: boolean;
  protected readonly employees = signal<Employee[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected readonly showForm = signal(false);
  protected readonly appraisalCandidates = signal<AppraisalCandidate[]>([]);
  protected readonly appraisalLoading = signal(false);
  protected readonly appraisalError = signal('');
  protected readonly reportsLoading = signal(false);
  protected readonly reportsError = signal('');
  protected readonly reportUpcoming = signal<ReportRow[]>([]);
  protected readonly reportInProcess = signal<ReportRow[]>([]);
  protected readonly reportCompleted = signal<ReportRow[]>([]);
  protected readonly reportHasDetailedCtc = computed(() =>
    this.reportCompleted().some(row => row.totalCtc != null || !!row.currency)
  );
  protected readonly activeSection = signal<LandingSection>('employees');
  protected readonly formModel = signal<Employee | null>(null);
  protected readonly sectionLabel = computed(() => this.getSectionLabel(this.activeSection()));
  protected readonly accessRoles = signal<AccessRole[]>([]);
  protected readonly accessRolesLoading = signal(false);
  protected readonly accessDialogOpen = signal(false);
  protected readonly selectedAccessEmployee = signal<Employee | null>(null);
  protected readonly selectedRoleIds = signal<number[]>([]);
  protected readonly userActiveStates = signal<Record<number, boolean>>({});
  protected readonly accessSaveMessage = signal('');
  protected readonly accessSaveError = signal('');
  protected readonly selectedCommentEmployeeId = signal<number | null>(null);
  protected readonly achievements = signal('');
  protected readonly notAchieved = signal('');
  protected readonly suggestions = signal('');
  protected readonly commentsSaveMessage = signal('');
  protected readonly commentsSaveError = signal('');
  protected readonly commentsThreadLoading = signal(false);
  protected readonly commentsThread = signal<FeedbackComment[]>([]);
  protected readonly finalizeSelectedAppraisalId = signal<number | null>(null);
  protected readonly finalizePromotion = signal<'YES' | 'NO' | ''>('');
  protected readonly finalizeBasic = signal<number | null>(null);
  protected readonly finalizeDa = signal<number | null>(null);
  protected readonly finalizeHra = signal<number | null>(null);
  protected readonly finalizeFoodAllowance = signal<number | null>(null);
  protected readonly finalizePf = signal<number | null>(null);
  protected readonly finalizeCurrency = signal('');
  protected readonly finalizeCurrencyOptions = signal<string[]>([]);
  protected readonly finalizeNextAppraisalDate = signal('');
  protected readonly finalizeSaving = signal(false);
  protected readonly finalizeMessage = signal('');
  protected readonly finalizeError = signal('');
  protected readonly finalizeEligibilityLoading = signal(false);
  protected readonly finalizeCommentReadyMap = signal<Record<number, boolean>>({});
  protected readonly finalizedCtcLoading = signal(false);
  protected readonly finalizedCtcError = signal('');
  protected readonly finalizedCtc = signal<CtcSnapshotView | null>(null);
  protected readonly managerComments = signal<FeedbackComment[]>([]);
  protected readonly employeeFeedbackText = signal('');
  protected readonly employeeFeedbackMessage = signal('');
  protected readonly employeeFeedbackError = signal('');
  protected readonly managerAssignedCandidates = computed(() =>
    this.appraisalCandidates().filter(item => item.category === 'ASSIGNED')
  );
  protected readonly finalizeEligibleCandidates = computed(() =>
    this.managerAssignedCandidates().filter(item => {
      const id = item.assignmentId;
      return typeof id === 'number' && !!this.finalizeCommentReadyMap()[id];
    })
  );
  protected readonly selectedCommentEmployeeProfile = computed(() =>
    this.employees().find(emp => emp.id === this.selectedCommentEmployeeId()) ?? null
  );
  protected readonly selectedUpcomingEmployeeId = signal<number | null>(null);
  protected readonly upcomingProfilePopupOpen = signal(false);
  protected readonly selectedUpcomingCandidate = computed(() =>
    this.managerAssignedCandidates().find(item => item.employeeId === this.selectedUpcomingEmployeeId()) ?? null
  );
  protected readonly selectedUpcomingEmployeeProfile = computed(() =>
    this.employees().find(emp => emp.id === this.selectedUpcomingEmployeeId()) ?? null
  );
  protected readonly employeeAssignedCandidates = computed(() =>
    this.appraisalCandidates().filter(item => item.category === 'ASSIGNED')
  );
  protected readonly selectedEmployeeAppraisalId = signal<number | null>(null);
  protected readonly selectedEmployeeAppraisalCandidate = computed(() =>
    this.employeeAssignedCandidates().find(item => item.assignmentId === this.selectedEmployeeAppraisalId()) ?? null
  );
  protected readonly feedbackLockedAfterFinal = computed(() => {
    const status = this.selectedEmployeeAppraisalCandidate()?.assignmentStatus ?? '';
    return status.trim().toUpperCase() === 'FINAL';
  });
  private readonly currentUserId: number | null;

  constructor(
    private readonly auth: AuthService,
    private readonly employeeService: EmployeeService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly destroyRef: DestroyRef
  ) {
    const normalizedRoles = this.normalizeRoles(this.auth.user?.roles);
    this.isHrUser = normalizedRoles.includes('HR');
    this.isManagerUser = normalizedRoles.includes('MANAGER');
    this.isEmployeeUser = normalizedRoles.includes('EMPLOYEE');
    this.currentUserId = this.parseUserId(this.auth.user?.userId);

    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const view = this.normalizeSection(params.get('view'));
        this.activeSection.set(view);
        if ((view === 'employees' || view === 'access') && this.isHrUser) {
          this.showForm.set(false);
          this.refreshEmployees();
        }
        if (view === 'access' && this.isHrUser) {
          this.loadAccessRoles();
          this.closeAccessDialog();
        }
        if ((view === 'appraisals' && this.isHrUser)
          || (view === 'upcoming' && this.isManagerUser)
          || (view === 'my-appraisal' && this.isEmployeeUser)
          || (view === 'feedback' && this.isEmployeeUser)) {
          this.refreshAppraisals();
        }

        if (view === 'reports') {
          this.loadReports();
        }

        if (view === 'finalize' && this.isManagerUser) {
          this.refreshAppraisals();
          this.loadSupportedCurrencies();
          this.resetFinalizeState();
        }

        if (view === 'upcoming' && this.isManagerUser) {
          this.loadManagerEmployeeProfiles();
        }

        if (view === 'comments' && this.isManagerUser) {
          this.refreshAppraisals();
          this.loadManagerEmployeeProfiles();
          this.resetCommentsState();
        }

        if ((view === 'my-appraisal' || view === 'feedback') && this.isEmployeeUser) {
          this.resetEmployeeFeedbackState();
        }
      });
  }

  protected onAddEmployee(): void {
    this.formModel.set({
      id: 0,
      name: '',
      address: '',
      city: '',
      phone: '',
      mobile: '',
      email: '',
      dob: null,
      gender: '',
      maritalStatus: '',
      doj: null,
      passport: '',
      pan: '',
      workExperience: null,
      reportsToId: null,
      reportsToName: '',
      departmentId: null,
      departmentName: '',
      orgUnitCode: '',
      managerId: null
    });
    this.showForm.set(true);
  }

  protected onFormCancel(): void {
    this.showForm.set(false);
    this.formModel.set(null);
  }

  protected onFormSubmit(employee: Employee | undefined): void {
    if (!employee) {
      return;
    }
    const payload: Partial<Employee> = { ...employee };
    delete payload.id;
    payload.managerId = payload.reportsToId ?? null;

    this.loading.set(true);
    this.employeeService.createEmployee(payload).subscribe({
      next: () => {
        this.loading.set(false);
        this.showForm.set(false);
        this.formModel.set(null);
        this.refreshEmployees();
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Unable to add employee right now.');
      }
    });
  }

  protected goToSection(section: LandingSection): void {
    this.router.navigate(['/landing'], { queryParams: { view: section } });
  }

  protected onCommentEmployeeChange(employeeIdValue: number | null): void {
    const employeeId = Number(employeeIdValue);
    const resolvedEmployeeId = Number.isFinite(employeeId) && employeeId > 0 ? employeeId : null;
    this.selectedCommentEmployeeId.set(resolvedEmployeeId);
    this.commentsSaveMessage.set('');
    this.commentsSaveError.set('');
    if (!resolvedEmployeeId) {
      this.commentsThread.set([]);
      return;
    }
    this.loadCommentsThreadForEmployee(resolvedEmployeeId);
  }

  protected onAchievementsChange(value: string): void {
    this.achievements.set(value);
  }

  protected onNotAchievedChange(value: string): void {
    this.notAchieved.set(value);
  }

  protected onSuggestionsChange(value: string): void {
    this.suggestions.set(value);
  }

  protected saveManagerComments(): void {
    this.commentsSaveMessage.set('');
    this.commentsSaveError.set('');

    if (!this.selectedCommentEmployeeId()) {
      this.commentsSaveError.set('Select an employee appraisal before saving comments.');
      return;
    }

    const selected = this.managerAssignedCandidates().find(
      item => item.employeeId === this.selectedCommentEmployeeId()
    );

    if (!selected?.assignmentId) {
      this.commentsSaveError.set('Assigned appraisal was not found for the selected employee.');
      return;
    }

    const managerText = [
      `Achievements: ${this.achievements().trim() || 'N/A'}`,
      `Things Not Achieved: ${this.notAchieved().trim() || 'N/A'}`,
      `Suggestions: ${this.suggestions().trim() || 'N/A'}`
    ].join('\n');

    const authorId = this.currentUserId;
    if (!authorId) {
      this.commentsSaveError.set('Unable to determine manager identity for saving comments.');
      return;
    }

    this.employeeService.saveManagerComment({
      appraisalId: selected.assignmentId,
      commentText: managerText,
      authorId
    }).subscribe({
      next: () => {
        this.commentsSaveMessage.set('Comments saved.');
        const selectedEmployeeId = this.selectedCommentEmployeeId();
        if (selectedEmployeeId) {
          this.loadCommentsThreadForEmployee(selectedEmployeeId);
        }
      },
      error: (err) => {
        const backendMessage = err?.error?.error as string | undefined;
        this.commentsSaveError.set(backendMessage || 'Unable to save manager comments.');
      }
    });
  }

  protected onEmployeeAppraisalChange(appraisalIdValue: number | null): void {
    const appraisalId = Number(appraisalIdValue);
    const resolvedId = Number.isFinite(appraisalId) && appraisalId > 0 ? appraisalId : null;
    this.selectedEmployeeAppraisalId.set(resolvedId);
    this.employeeFeedbackMessage.set('');
    this.employeeFeedbackError.set('');
    this.managerComments.set([]);

    if (!resolvedId) {
      return;
    }

    this.loadManagerComments(resolvedId);
  }

  protected onEmployeeFeedbackChange(value: string): void {
    this.employeeFeedbackText.set(value);
    this.employeeFeedbackMessage.set('');
    this.employeeFeedbackError.set('');
  }

  protected onFinalizeAppraisalChange(appraisalIdValue: number | null): void {
    const appraisalId = Number(appraisalIdValue);
    const resolvedId = Number.isFinite(appraisalId) && appraisalId > 0 ? appraisalId : null;
    this.finalizeSelectedAppraisalId.set(resolvedId);
    this.finalizeMessage.set('');
    this.finalizeError.set('');
    this.finalizedCtc.set(null);
    this.finalizedCtcError.set('');

    if (!resolvedId) {
      this.finalizedCtcLoading.set(false);
      return;
    }

    this.loadFinalizedCtc(resolvedId);
  }

  protected saveFinalizeAndCtc(): void {
    this.finalizeMessage.set('');
    this.finalizeError.set('');

    const appraisalId = this.finalizeSelectedAppraisalId();
    if (!appraisalId) {
      this.finalizeError.set('Select an appraisal before finalizing.');
      return;
    }

    const selected = this.managerAssignedCandidates().find(item => item.assignmentId === appraisalId);
    if (!selected) {
      this.finalizeError.set('Selected appraisal is not available in assigned list.');
      return;
    }

    if (!this.finalizeCommentReadyMap()[appraisalId]) {
      this.finalizeError.set('Manager comments are required before finalization.');
      return;
    }

    const promotion = this.finalizePromotion();
    if (!promotion) {
      this.finalizeError.set('Select promotion decision (Yes/No).');
      return;
    }

    const basic = this.finalizeBasic();
    const da = this.finalizeDa();
    const hra = this.finalizeHra();
    const food = this.finalizeFoodAllowance();
    const pf = this.finalizePf();
    const nextDate = this.finalizeNextAppraisalDate().trim();
    const currency = this.finalizeCurrency().trim();

    const components = [basic, da, hra, food, pf];
    if (components.some(value => value == null || !Number.isFinite(value) || value < 0)) {
      this.finalizeError.set('All CTC component fields are required and must be non-negative.');
      return;
    }

    if (!nextDate) {
      this.finalizeError.set('Next appraisal date is required.');
      return;
    }

    if (!currency) {
      this.finalizeError.set('Currency is required.');
      return;
    }

    const totalCtc = Number((basic! + da! + hra! + food! + pf!).toFixed(2));
    this.finalizeSaving.set(true);

    this.employeeService.saveCtcDecision({
      employeeId: selected.employeeId,
      appraisalId,
      ctcAmount: totalCtc,
      currency
    }).subscribe({
      next: () => {
        this.employeeService.finalizeAppraisal(appraisalId, {
          promotionRecommended: promotion === 'YES',
          nextAppraisalDate: nextDate
        }).subscribe({
          next: () => {
            this.finalizeSaving.set(false);
            this.finalizeMessage.set('Appraisal finalized and CTC saved successfully.');
            this.loadFinalizedCtc(appraisalId);
            this.refreshAppraisals();
          },
          error: (err) => {
            const backendMessage = err?.error?.message as string | undefined;
            this.finalizeSaving.set(false);
            this.finalizeError.set(backendMessage || 'CTC saved, but finalization failed.');
          }
        });
      },
      error: (err) => {
        const backendMessage = err?.error?.message as string | undefined;
        this.finalizeSaving.set(false);
        this.finalizeError.set(backendMessage || 'Unable to save CTC details right now.');
      }
    });
  }

  protected submitEmployeeFeedback(): void {
    this.employeeFeedbackMessage.set('');
    this.employeeFeedbackError.set('');

    const appraisalId = this.selectedEmployeeAppraisalId();
    if (!appraisalId) {
      this.employeeFeedbackError.set('Select an appraisal before submitting feedback.');
      return;
    }

    if (!this.managerComments().length) {
      this.employeeFeedbackError.set('Manager comments are not available yet.');
      return;
    }

    if (this.feedbackLockedAfterFinal()) {
      this.employeeFeedbackError.set('Feedback is closed because this appraisal has been finalized.');
      return;
    }

    const feedback = this.employeeFeedbackText().trim();
    if (feedback.length < 3) {
      this.employeeFeedbackError.set('Feedback must be at least 3 characters.');
      return;
    }

    const authorId = this.currentUserId;
    if (!authorId) {
      this.employeeFeedbackError.set('Unable to determine employee identity for feedback submission.');
      return;
    }

    this.employeeService.submitEmployeeFeedback({
      appraisalId,
      commentText: feedback,
      authorId
    }).subscribe({
      next: () => {
        this.employeeFeedbackMessage.set('Feedback submitted successfully.');
        this.employeeFeedbackText.set('');
      },
      error: (err) => {
        const backendMessage = err?.error?.error as string | undefined;
        this.employeeFeedbackError.set(backendMessage || 'Unable to submit feedback right now.');
      }
    });
  }

  protected onAssignAppraisal(candidate: AppraisalCandidate): void {
    this.appraisalLoading.set(true);
    this.appraisalError.set('');
    this.employeeService.assignUpcomingAppraisal({
      employeeId: candidate.employeeId,
      managerId: candidate.assignedManagerId ?? candidate.reportsToId,
      cycleName: candidate.cycleName
    }).subscribe({
      next: () => {
        this.refreshAppraisals();
      },
      error: (err) => {
        const backendMessage = err?.error?.message as string | undefined;
        this.appraisalError.set(backendMessage || 'Unable to assign appraisal.');
        this.appraisalLoading.set(false);
      }
    });
  }

  protected onViewUpcomingCandidate(candidate: AppraisalCandidate): void {
    this.selectedUpcomingEmployeeId.set(candidate.employeeId);
    this.upcomingProfilePopupOpen.set(true);
    if (!this.employees().length) {
      this.loadManagerEmployeeProfiles();
    }
  }

  protected closeUpcomingProfilePopup(): void {
    this.upcomingProfilePopupOpen.set(false);
  }

  protected onUserStatusToggle(payload: { employee: Employee; active: boolean }): void {
    const { employee, active } = payload;
    this.accessSaveMessage.set('');
    this.accessSaveError.set('');
    const previous = this.userActiveStates()[employee.id] ?? false;
    this.setUserActiveState(employee.id, active);

    this.employeeService.updateUserStatus(employee.id, active).subscribe({
      next: () => {
        this.accessSaveMessage.set(`${active ? 'Activated' : 'Deactivated'} ${employee.name}.`);
      },
      error: (err) => {
        const backendMessage = err?.error?.error as string | undefined;
        this.setUserActiveState(employee.id, previous);
        this.accessSaveError.set(backendMessage || `Unable to update status for ${employee.name}.`);
      }
    });
  }

  protected openAssignRoleDialog(employee: Employee): void {
    this.accessSaveMessage.set('');
    this.accessSaveError.set('');
    this.selectedAccessEmployee.set(employee);
    this.selectedRoleIds.set([]);
    this.accessDialogOpen.set(true);

    this.employeeService.getUserAssignedRoleIds(employee.id).subscribe({
      next: response => {
        this.selectedRoleIds.set(response.roleIds ?? []);
      },
      error: () => {
        this.selectedRoleIds.set([]);
      }
    });
  }

  protected toggleRoleSelection(roleId: number, checked: boolean): void {
    const current = this.selectedRoleIds();
    if (checked) {
      if (!current.includes(roleId)) {
        this.selectedRoleIds.set([...current, roleId]);
      }
      return;
    }
    this.selectedRoleIds.set(current.filter(id => id !== roleId));
  }

  protected saveAssignedRoles(): void {
    const selected = this.selectedAccessEmployee();
    if (!selected) {
      this.accessSaveError.set('No employee selected for role assignment.');
      return;
    }

    this.employeeService.updateUserRoles(selected.id, this.selectedRoleIds()).subscribe({
      next: () => {
        this.accessSaveMessage.set(`Roles updated for ${selected.name}.`);
        this.closeAccessDialog();
      },
      error: (err) => {
        const backendMessage = err?.error?.error as string | undefined;
        this.accessSaveError.set(backendMessage || `Unable to update roles for ${selected.name}.`);
      }
    });
  }

  protected closeAccessDialog(): void {
    this.accessDialogOpen.set(false);
    this.selectedAccessEmployee.set(null);
    this.selectedRoleIds.set([]);
  }

  private refreshEmployees(): void {
    this.loading.set(true);
    this.error.set('');
    this.employeeService.getAllEmployees().subscribe({
      next: employees => {
        this.employees.set(employees);
        const ids = employees
          .map(emp => emp.id)
          .filter(id => Number.isFinite(id) && id > 0);

        if (!ids.length) {
          this.userActiveStates.set({});
          this.loading.set(false);
          return;
        }

        this.employeeService.getUserActiveStatuses(ids).subscribe({
          next: statusMap => {
            this.userActiveStates.set(statusMap ?? {});
            this.loading.set(false);
          },
          error: () => {
            this.userActiveStates.set({});
            this.loading.set(false);
          }
        });
      },
      error: () => {
        this.error.set('Unable to load employee directory.');
        this.loading.set(false);
      }
    });
  }

  private loadManagerEmployeeProfiles(): void {
    this.employeeService.getAllEmployees().subscribe({
      next: employees => {
        this.employees.set(employees);
      },
      error: () => {
        this.employees.set([]);
      }
    });
  }

  private setUserActiveState(employeeId: number, active: boolean): void {
    const current = this.userActiveStates();
    this.userActiveStates.set({
      ...current,
      [employeeId]: active
    });
  }

  private refreshAppraisals(): void {
    this.appraisalLoading.set(true);
    this.appraisalError.set('');
    const managerUserId = this.isManagerUser ? this.currentUserId ?? undefined : undefined;
    const employeeUserId = this.isEmployeeUser ? this.currentUserId ?? undefined : undefined;
    this.employeeService.getUpcomingAppraisals(undefined, managerUserId, employeeUserId).subscribe({
      next: candidates => {
        if (this.isManagerUser) {
          const assigned = candidates.filter(item => item.category === 'ASSIGNED');
          this.appraisalCandidates.set(assigned);
          if (this.activeSection() === 'finalize') {
            this.loadFinalizeEligibility(assigned);
          }
          if (!assigned.length) {
            this.selectedUpcomingEmployeeId.set(null);
            this.upcomingProfilePopupOpen.set(false);
          } else {
            const currentSelection = this.selectedUpcomingEmployeeId();
            const exists = assigned.some(item => item.employeeId === currentSelection);
            this.selectedUpcomingEmployeeId.set(exists ? currentSelection : assigned[0].employeeId);
          }
        } else if (this.isEmployeeUser) {
          const assigned = candidates.filter(item => item.category === 'ASSIGNED');
          this.appraisalCandidates.set(assigned);
          if (assigned.length > 0) {
            const selectedId = this.selectedEmployeeAppraisalId();
            const current = assigned.find(item => item.assignmentId === selectedId);
            const fallback = current?.assignmentId ?? assigned[0].assignmentId ?? null;
            this.selectedEmployeeAppraisalId.set(fallback);
            if (fallback) {
              this.loadManagerComments(fallback);
            }
          } else {
            this.selectedEmployeeAppraisalId.set(null);
            this.managerComments.set([]);
          }
        } else {
          this.appraisalCandidates.set(candidates);
        }
        this.appraisalLoading.set(false);
      },
      error: () => {
        this.appraisalError.set('Unable to load upcoming appraisals.');
        this.appraisalLoading.set(false);
      }
    });
  }

  private loadAccessRoles(): void {
    if (this.accessRoles().length > 0) {
      return;
    }
    this.accessRolesLoading.set(true);
    this.employeeService.getAccessRoles().subscribe({
      next: roles => {
        this.accessRoles.set(roles);
        this.accessRolesLoading.set(false);
      },
      error: () => {
        this.accessRolesLoading.set(false);
      }
    });
  }

  private loadReports(): void {
    this.reportsLoading.set(true);
    this.reportsError.set('');

    forkJoin({
      upcoming: this.employeeService.getUpcomingReport(),
      inProcess: this.employeeService.getInProcessReport(),
      completed: this.employeeService.getCompletedReport()
    }).subscribe({
      next: ({ upcoming, inProcess, completed }) => {
        this.reportUpcoming.set(upcoming ?? []);
        this.reportInProcess.set(inProcess ?? []);
        this.reportCompleted.set(completed ?? []);
        this.reportsLoading.set(false);
      },
      error: (err) => {
        const backendMessage = err?.error?.message as string | undefined;
        this.reportUpcoming.set([]);
        this.reportInProcess.set([]);
        this.reportCompleted.set([]);
        this.reportsError.set(backendMessage || 'Unable to load reports right now.');
        this.reportsLoading.set(false);
      }
    });
  }

  private normalizeSection(view: string | null): LandingSection {
    if (this.isManagerUser) {
      if (view === 'reports') {
        return 'reports';
      }
      if (view === 'comments') {
        return 'comments';
      }
      if (view === 'finalize') {
        return 'finalize';
      }
      return 'upcoming';
    }

    if (this.isEmployeeUser) {
      if (view === 'reports') {
        return 'reports';
      }
      if (view === 'feedback') {
        return 'feedback';
      }
      return 'my-appraisal';
    }

    if (view === 'appraisals' || view === 'reports' || view === 'access') {
      return view;
    }
    return 'employees';
  }

  private getSectionLabel(section: LandingSection): string {
    switch (section) {
      case 'employees':
        return 'Manage Employees';
      case 'appraisals':
        return 'Appraisals';
      case 'reports':
        return 'Reports';
      case 'access':
        return 'Manage Access';
      case 'upcoming':
        return 'Upcoming Appraisals';
      case 'comments':
        return 'Manager Comments';
      case 'finalize':
        return 'Finalize & CTC';
      case 'my-appraisal':
        return 'My Appraisal';
      case 'feedback':
        return 'Submit Feedback';
      default:
        return 'Workspace';
    }
  }

  private resetCommentsState(): void {
    this.selectedCommentEmployeeId.set(null);
    this.commentsThread.set([]);
    this.commentsThreadLoading.set(false);
    this.achievements.set('');
    this.notAchieved.set('');
    this.suggestions.set('');
    this.commentsSaveMessage.set('');
    this.commentsSaveError.set('');
  }

  private resetFinalizeState(): void {
    this.finalizeSelectedAppraisalId.set(null);
    this.finalizePromotion.set('');
    this.finalizeBasic.set(null);
    this.finalizeDa.set(null);
    this.finalizeHra.set(null);
    this.finalizeFoodAllowance.set(null);
    this.finalizePf.set(null);
    this.finalizeCurrency.set('');
    this.finalizeNextAppraisalDate.set('');
    this.finalizeSaving.set(false);
    this.finalizeMessage.set('');
    this.finalizeError.set('');
    this.finalizeEligibilityLoading.set(false);
    this.finalizeCommentReadyMap.set({});
    this.finalizedCtcLoading.set(false);
    this.finalizedCtcError.set('');
    this.finalizedCtc.set(null);
  }

  private loadFinalizeEligibility(candidates: AppraisalCandidate[]): void {
    const assignmentIds = candidates
      .map(item => item.assignmentId)
      .filter((id): id is number => typeof id === 'number' && Number.isFinite(id) && id > 0);

    if (!assignmentIds.length) {
      this.finalizeCommentReadyMap.set({});
      this.finalizeEligibilityLoading.set(false);
      return;
    }

    this.finalizeEligibilityLoading.set(true);

    const checks = assignmentIds.map(assignmentId =>
      this.employeeService.getManagerComments(assignmentId).pipe(
        map(comments => ({ assignmentId, ready: comments.length > 0 })),
        catchError(() => of({ assignmentId, ready: false }))
      )
    );

    forkJoin(checks).subscribe({
      next: rows => {
        const nextMap: Record<number, boolean> = {};
        rows.forEach(row => {
          nextMap[row.assignmentId] = row.ready;
        });
        this.finalizeCommentReadyMap.set(nextMap);
        this.finalizeEligibilityLoading.set(false);
      },
      error: () => {
        this.finalizeCommentReadyMap.set({});
        this.finalizeEligibilityLoading.set(false);
      }
    });
  }

  private loadSupportedCurrencies(): void {
    if (this.finalizeCurrencyOptions().length) {
      return;
    }

    this.employeeService.getSupportedCurrencies().subscribe({
      next: currencies => {
        this.finalizeCurrencyOptions.set(currencies);
      },
      error: () => {
        this.finalizeCurrencyOptions.set([]);
      }
    });
  }

  private loadFinalizedCtc(appraisalId: number): void {
    this.finalizedCtcLoading.set(true);
    this.finalizedCtcError.set('');

    this.employeeService.getCtcByAppraisal(appraisalId).subscribe({
      next: rows => {
        this.finalizedCtc.set(rows.length ? rows[0] : null);
        this.finalizedCtcLoading.set(false);
      },
      error: (err) => {
        const backendMessage = err?.error?.message as string | undefined;
        this.finalizedCtc.set(null);
        this.finalizedCtcError.set(backendMessage || 'Unable to load finalized CTC details.');
        this.finalizedCtcLoading.set(false);
      }
    });
  }

  private loadCommentsThreadForEmployee(employeeId: number): void {
    const selected = this.managerAssignedCandidates().find(item => item.employeeId === employeeId);
    if (!selected?.assignmentId) {
      this.commentsThread.set([]);
      return;
    }

    this.commentsThreadLoading.set(true);
    this.employeeService.getAllComments(selected.assignmentId).subscribe({
      next: comments => {
        const sorted = [...comments].sort((left, right) => {
          const leftTime = left.createdAt ? new Date(left.createdAt).getTime() : 0;
          const rightTime = right.createdAt ? new Date(right.createdAt).getTime() : 0;
          return leftTime - rightTime;
        });
        this.commentsThread.set(sorted);
        this.commentsThreadLoading.set(false);
      },
      error: () => {
        this.commentsThread.set([]);
        this.commentsThreadLoading.set(false);
      }
    });
  }

  protected resolveCommentAuthor(comment: FeedbackComment): string {
    if (comment.authorId === this.currentUserId) {
      return 'Manager';
    }
    return 'Employee';
  }

  private resetEmployeeFeedbackState(): void {
    this.employeeFeedbackText.set('');
    this.employeeFeedbackMessage.set('');
    this.employeeFeedbackError.set('');
    this.managerComments.set([]);
  }

  private loadManagerComments(appraisalId: number): void {
    this.employeeService.getManagerComments(appraisalId).subscribe({
      next: comments => {
        this.managerComments.set(comments);
      },
      error: () => {
        this.managerComments.set([]);
      }
    });
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

  private parseUserId(rawUserId: unknown): number | null {
    const numericUserId = Number(rawUserId);
    if (!Number.isFinite(numericUserId) || numericUserId <= 0) {
      return null;
    }
    return numericUserId;
  }
}
