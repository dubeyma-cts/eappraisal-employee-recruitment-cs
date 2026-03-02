import { Component } from '@angular/core';

@Component({
  selector: 'app-hr-menu',
  template: `
    <nav class="left-menu">
      <ul>
        <li><a routerLink="/hr/employees" routerLinkActive="active">Manage Employees</a></li>
        <li><a routerLink="/hr/orgunits" routerLinkActive="active">Manage Org Units</a></li>
        <li><a routerLink="/hr/reports" routerLinkActive="active">Reports</a></li>
      </ul>
    </nav>
  `,
  styles: [`
    .left-menu {
      width: 220px;
      background: #f5f5f5;
      height: 100vh;
      padding-top: 20px;
      position: fixed;
    }
    .left-menu ul {
      list-style: none;
      padding: 0;
    }
    .left-menu li {
      margin-bottom: 16px;
    }
    .left-menu a {
      text-decoration: none;
      color: #333;
      font-weight: 500;
      padding: 8px 16px;
      display: block;
      border-radius: 4px;
    }
    .left-menu a.active {
      background: #1976d2;
      color: #fff;
    }
  `]
})
export class HrMenuComponent {}
