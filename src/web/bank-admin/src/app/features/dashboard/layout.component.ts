import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatIconModule,
    MatListModule,
    MatToolbarModule,
    MatMenuModule,
    MatButtonModule
  ],
  template: `
    <mat-sidenav-container class="container">
      <mat-sidenav mode="side" opened>
        <mat-toolbar color="primary">
          <mat-icon>account_balance</mat-icon>
          <span>BankSystem V2</span>
        </mat-toolbar>
        <mat-nav-list>
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>Dashboard</span>
          </a>
          <a mat-list-item routerLink="/customers" routerLinkActive="active">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>Customers</span>
          </a>
          <a mat-list-item routerLink="/accounts" routerLinkActive="active">
            <mat-icon matListItemIcon>account_balance_wallet</mat-icon>
            <span matListItemTitle>Accounts</span>
          </a>
          <a mat-list-item routerLink="/transactions" routerLinkActive="active">
            <mat-icon matListItemIcon>swap_horiz</mat-icon>
            <span matListItemTitle>Transactions</span>
          </a>
          <a mat-list-item routerLink="/users" routerLinkActive="active" *ngIf="isSuperAdmin">
            <mat-icon matListItemIcon>manage_accounts</mat-icon>
            <span matListItemTitle>Users</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>
      
      <mat-sidenav-content>
        <mat-toolbar color="primary" class="header">
          <span class="spacer"></span>
          <button mat-button [matMenuTriggerFor]="menu">
            <mat-icon>account_circle</mat-icon>
            {{user?.firstName}} {{user?.lastName}}
          </button>
          <mat-menu #menu="matMenu">
            <button mat-menu-item disabled>
              <mat-icon>badge</mat-icon>
              <span>{{user?.role}}</span>
            </button>
            <button mat-menu-item (click)="logout()">
              <mat-icon>logout</mat-icon>
              <span>Logout</span>
            </button>
          </mat-menu>
        </mat-toolbar>
        
        <div class="content">
          <router-outlet></router-outlet>
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .container {
      height: 100vh;
    }
    mat-sidenav {
      width: 250px;
    }
    mat-toolbar {
      display: flex;
    }
    .spacer {
      flex: 1;
    }
    .header {
      justify-content: flex-end;
    }
    .content {
      padding: 20px;
    }
    mat-nav-list a.active {
      background-color: rgba(0,0,0,0.1);
    }
  `]
})
export class LayoutComponent {
  constructor(private authService: AuthService) {}
  
  get user() {
    return this.authService.currentUser();
  }
  
  get isSuperAdmin() {
    return this.authService.hasRole(['SuperAdmin']);
  }
  
  logout() {
    this.authService.logout();
  }
}
