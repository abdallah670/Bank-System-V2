import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { ApiService, Customer } from '../../core/services/api.service';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatInputModule, MatFormFieldModule, FormsModule],
  template: `
    <h1>Customers</h1>
    
    <mat-card>
      <mat-card-content>
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search</mat-label>
          <input matInput [(ngModel)]="searchTerm" (input)="onSearch()">
          <mat-icon matSuffix>search</mat-icon>
        </mat-form-field>
        
        <table mat-table [dataSource]="customers" class="customers-table">
          <ng-container matColumnDef="id">
            <th mat-header-cell *matHeaderCellDef>ID</th>
            <td mat-cell *matCellDef="let c">{{c.customerId}}</td>
          </ng-container>
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let c">{{c.firstName}} {{c.lastName}}</td>
          </ng-container>
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef>Email</th>
            <td mat-cell *matCellDef="let c">{{c.email}}</td>
          </ng-container>
          <ng-container matColumnDef="phone">
            <th mat-header-cell *matHeaderCellDef>Phone</th>
            <td mat-cell *matCellDef="let c">{{c.phone}}</td>
          </ng-container>
          <ng-container matColumnDef="accounts">
            <th mat-header-cell *matHeaderCellDef>Accounts</th>
            <td mat-cell *matCellDef="let c">{{c.accounts?.length || 0}}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let c">
              <span class="status-badge" [class.active]="c.isActive">{{c.isActive ? 'Active' : 'Inactive'}}</span>
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
        
        <mat-paginator [length]="totalCount" [pageSize]="pageSize" [pageSizeOptions]="[5, 10, 25]"
          (page)="onPageChange($event)"></mat-paginator>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .search-field { width: 300px; margin-bottom: 20px; }
    .customers-table { width: 100%; }
    .status-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      background: #ffebee;
      color: #f44336;
    }
    .status-badge.active {
      background: #e8f5e9;
      color: #4caf50;
    }
  `]
})
export class CustomersComponent implements OnInit {
  customers: Customer[] = [];
  displayedColumns = ['id', 'name', 'email', 'phone', 'accounts', 'status'];
  totalCount = 0;
  pageSize = 10;
  currentPage = 1;
  searchTerm = '';

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.apiService.getCustomers({ page: this.currentPage, pageSize: this.pageSize, search: this.searchTerm }).subscribe({
      next: (res) => {
        this.customers = res.items;
        this.totalCount = res.totalCount;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadCustomers();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadCustomers();
  }
}
