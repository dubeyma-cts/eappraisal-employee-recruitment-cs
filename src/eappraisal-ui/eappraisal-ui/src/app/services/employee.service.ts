import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

export interface Employee {
  id: number;
  name: string;
  address: string;
  city: string;
  phone: string;
  mobile: string;
  email: string;
  dob: string | null;
  gender: string;
  maritalStatus: string;
  doj: string | null;
  passport: string;
  pan: string;
  workExperience: number | null;
  reportsToId: number | null;
  reportsToName: string;
  departmentId: number | null;
  departmentName: string;
  orgUnitCode: string;
  managerId: number | null;
}

export interface DepartmentOption {
  id: number;
  name: string;
}

export interface ManagerOption {
  id: number;
  name: string;
  displayLabel: string;
}

export interface AppraisalCandidate {
  employeeId: number;
  employeeName: string;
  employeeEmail: string;
  departmentName: string;
  reportsToId: number | null;
  reportsToName: string;
  dueDate: string;
  cycleName: string;
  category: 'ELIGIBLE' | 'BLOCKED' | 'ASSIGNED';
  assignmentStatus: string;
  assignmentId: number | null;
  assignedManagerId: number | null;
  assignedManagerName: string;
  blockerReason: string;
}

export interface AppraisalAssignRequest {
  employeeId: number;
  managerId?: number | null;
  cycleName?: string;
}

export interface FeedbackComment {
  id: number;
  appraisalId: number;
  commentText: string;
  authorId: number;
  visibility: string;
  createdAt: string;
}

export interface CommentSaveRequest {
  appraisalId: number;
  commentText: string;
  authorId: number;
}

export interface AccessRole {
  id: number;
  name: string;
}

export interface UserRoleAssignmentResponse {
  userId: number;
  roleIds: number[];
  message?: string;
}

export interface UserStatusResponse {
  userId: number;
  status: string;
}

export interface FinalizeAppraisalRequest {
  promotionRecommended?: boolean;
  nextAppraisalDate?: string;
}

export interface CtcDecisionRequest {
  employeeId: number;
  appraisalId: number;
  ctcAmount: number;
  currency: string;
}

export interface CtcSnapshotView {
  id: number;
  employeeId: number;
  appraisalId: number;
  ctcAmount: number;
  createdAt: string;
}

export interface ReportRow {
  appraisalId: number;
  cycleName: string;
  employeeId: number;
  employeeName: string;
  managerId: number;
  managerName: string;
  status: string;
  cycleEndDate: string;
  finalizedAt: string;
  totalCtc: number | null;
  currency: string | null;
}

@Injectable({ providedIn: 'root' })
export class EmployeeService {
  private apiUrl = `${environment.api.employeeBaseUrl}/api/v1/employees`;
  private appraisalApiUrl = `${environment.api.appraisalBaseUrl}/api/v1/workflow/appraisals`;
  private feedbackApiUrl = `${environment.api.commentsBaseUrl}/api/v1/feedback/comments`;
  private identityAccessApiUrl = `${environment.api.identityBaseUrl}/api/v1/identity/access`;
  private ctcApiUrl = `${environment.api.ctcBaseUrl}/api/v1/ctc/decisions`;
  private reportApiUrl = `${environment.api.reportBaseUrl}/api/v1/reports`;

  constructor(private http: HttpClient, private auth: AuthService) {}

  getEmployeeByEmail(email: string): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${email}`);
  }

  getAllEmployees(): Observable<Employee[]> {
    return this.http.get<Employee[]>(this.apiUrl);
  }

  getDepartments(): Observable<DepartmentOption[]> {
    return this.http.get<DepartmentOption[]>(`${this.apiUrl}/departments`);
  }

  searchManagers(query: string): Observable<ManagerOption[]> {
    return this.http.get<ManagerOption[]>(`${this.apiUrl}/managers/search`, {
      params: { q: query }
    });
  }

  createEmployee(employee: Partial<Employee>): Observable<Employee> {
    return this.http.post<Employee>(this.apiUrl, employee);
  }

  getUpcomingAppraisals(cycle?: string, managerUserId?: number, employeeUserId?: number): Observable<AppraisalCandidate[]> {
    let params = new HttpParams();
    if (cycle) {
      params = params.set('cycle', cycle);
    }
    if (typeof managerUserId === 'number' && Number.isFinite(managerUserId) && managerUserId > 0) {
      params = params.set('managerUserId', managerUserId);
    }
    if (typeof employeeUserId === 'number' && Number.isFinite(employeeUserId) && employeeUserId > 0) {
      params = params.set('employeeUserId', employeeUserId);
    }
    return this.http.get<AppraisalCandidate[]>(`${this.appraisalApiUrl}/upcoming`, { params });
  }

  assignUpcomingAppraisal(request: AppraisalAssignRequest): Observable<AppraisalCandidate> {
    return this.http.post<AppraisalCandidate>(`${this.appraisalApiUrl}/assign`, request);
  }

  getManagerComments(appraisalId: number): Observable<FeedbackComment[]> {
    return this.http.get<FeedbackComment[]>(`${this.feedbackApiUrl}/appraisal/${appraisalId}/manager`);
  }

  getAllComments(appraisalId: number): Observable<FeedbackComment[]> {
    return this.http.get<FeedbackComment[]>(`${this.feedbackApiUrl}/appraisal/${appraisalId}`);
  }

  saveManagerComment(request: CommentSaveRequest): Observable<FeedbackComment> {
    return this.http.post<FeedbackComment>(`${this.feedbackApiUrl}/manager`, request);
  }

  submitEmployeeFeedback(request: CommentSaveRequest): Observable<FeedbackComment> {
    return this.http.post<FeedbackComment>(`${this.feedbackApiUrl}/employee-feedback`, request);
  }

  getAccessRoles(): Observable<AccessRole[]> {
    return this.http.get<AccessRole[]>(`${this.identityAccessApiUrl}/roles`);
  }

  getUserAssignedRoleIds(userId: number): Observable<UserRoleAssignmentResponse> {
    return this.http.get<UserRoleAssignmentResponse>(`${this.identityAccessApiUrl}/users/${userId}/roles`);
  }

  updateUserStatus(userId: number, active: boolean): Observable<UserStatusResponse> {
    return this.http.put<UserStatusResponse>(`${this.identityAccessApiUrl}/users/${userId}/status`, { active });
  }

  updateUserRoles(userId: number, roleIds: number[]): Observable<UserRoleAssignmentResponse> {
    return this.http.put<UserRoleAssignmentResponse>(`${this.identityAccessApiUrl}/users/${userId}/roles`, { roleIds });
  }

  getUserActiveStatuses(userIds: number[]): Observable<Record<number, boolean>> {
    let params = new HttpParams();
    userIds.filter(id => Number.isFinite(id) && id > 0).forEach(id => {
      params = params.append('userIds', String(id));
    });
    return this.http.get<Record<number, boolean>>(`${this.identityAccessApiUrl}/users/statuses`, { params });
  }

  saveCtcDecision(request: CtcDecisionRequest): Observable<unknown> {
    return this.http.post(this.ctcApiUrl, request, this.getUserContextRequestOptions());
  }

  getCtcByAppraisal(appraisalId: number): Observable<CtcSnapshotView[]> {
    return this.http.get<CtcSnapshotView[]>(`${this.ctcApiUrl}/appraisal/${appraisalId}`, this.getUserContextRequestOptions());
  }

  getSupportedCurrencies(): Observable<string[]> {
    return this.http.get<string[]>(`${this.ctcApiUrl}/currencies`, this.getUserContextRequestOptions());
  }

  getUpcomingReport(): Observable<ReportRow[]> {
    return this.http.get<ReportRow[]>(`${this.reportApiUrl}/upcoming`, this.getUserContextRequestOptions());
  }

  getInProcessReport(): Observable<ReportRow[]> {
    return this.http.get<ReportRow[]>(`${this.reportApiUrl}/in-process`, this.getUserContextRequestOptions());
  }

  getCompletedReport(): Observable<ReportRow[]> {
    return this.http.get<ReportRow[]>(`${this.reportApiUrl}/completed`, this.getUserContextRequestOptions());
  }

  finalizeAppraisal(appraisalId: number, request: FinalizeAppraisalRequest): Observable<unknown> {
    return this.http.put(`${this.appraisalApiUrl}/${appraisalId}/finalize`, request);
  }

  private getUserContextRequestOptions(): { headers: HttpHeaders } {
    const user = this.auth.user ?? {};
    const rawRoles = Array.isArray(user.roles)
      ? user.roles
      : typeof user.roles === 'string'
        ? user.roles.split(',')
        : [];

    const roles = rawRoles
      .filter((role: unknown): role is string => typeof role === 'string')
      .map((role: string) => role.trim().toUpperCase())
      .filter((role: string) => role.length > 0)
      .join(',');

    let headers = new HttpHeaders();
    if (user.userId != null) {
      headers = headers.set('X-User-Id', String(user.userId));
    }
    if (roles) {
      headers = headers.set('X-User-Roles', roles);
    }

    return { headers };
  }
}
