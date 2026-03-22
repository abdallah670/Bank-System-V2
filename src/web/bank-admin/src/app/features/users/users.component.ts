import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ApiService, User } from '../../core/services/api.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, FormsModule],
  template: `
    <h1>Users Management</h1>
    
    <mat-card>
      <mat-card-content>
        <table mat-table [dataSource]="users" class="users-table">
          <ng-container matColumnDef="id">
            <th mat-header-cell *matHeaderCellDef>ID</th>
            <td mat-cell *matCellDef="let u">{{u.userId}}</td>
          </ng-container>
          <ng-container matColumnDef="username">
            <th mat-header-cell *matHeaderCellDef>Username</th>
            <td mat-cell *matCellDef="let u">{{u.username}}</td>
          </ng-container>
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let u">{{u.firstName}} {{u.lastName}}</td>
          </ng-container>
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef>Email</th>
            <td mat-cell *matCellDef="let u">{{u.email}}</td>
          </ng-container>
          <ng-container matColumnDef="role">
            <th mat-header-cell *matHeaderCellDef>Role</th>
            <td mat-cell *matCellDef="let u">
              <span class="role-badge" [class]="u.role.toLowerCase()">{{u.role}}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="lastLogin">
            <th mat-header-cell *matHeaderCellDef>Last Login</th>
            <td mat-cell *matCellDef="let u">{{u.lastLoginAt ? (u.lastLoginAt | date:'short') : 'Never'}}</td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
        
        <mat-paginator [length]="totalCount" [pageSize]="pageSize" [pageSizeOptions]="[10, 25]"
          (page)="onPageChange($event)"></mat-paginator>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .users-table { width: 100%; }
    .role-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
    }
    .role-badge.superadmin { background: #f3e5f5; color: #9c27b0; }
    .role-badge.bankadmin { background: #e3f2fd; color: #2196f3; }
    .role-badge.auditor { background: #fff3e0; color: #ff9800; }
  `]
})
export class UsersComponent implements OnInit {
  users: User[] = [];
  displayedColumns = ['id', 'username', 'name', 'email', 'role', 'lastLogin'];
  totalCount = 0;
  pageSize = 10;

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.apiService.getUsers({ page: 1, pageSize: this.pageSize }).subscribe({
      next: (res) => {
        this.users = res.items;
        this.totalCount = res.totalCount;
      }
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageSize = event.pageSize;
    this.loadUsers();
  }
}
