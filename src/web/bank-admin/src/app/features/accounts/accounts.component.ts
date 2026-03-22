import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ApiService, Account } from '../../core/services/api.service';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, FormsModule],
  template: `
    <h1>Accounts</h1>
    
    <mat-card>
      <mat-card-content>
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search</mat-label>
          <input matInput [(ngModel)]="searchTerm" (input)="onSearch()">
        </mat-form-field>
        
        <table mat-table [dataSource]="accounts" class="accounts-table">
          <ng-container matColumnDef="number">
            <th mat-header-cell *matHeaderCellDef>Account Number</th>
            <td mat-cell *matCellDef="let a">{{a.accountNumber}}</td>
          </ng-container>
          <ng-container matColumnDef="customer">
            <th mat-header-cell *matHeaderCellDef>Customer</th>
            <td mat-cell *matCellDef="let a">{{a.customerName}}</td>
          </ng-container>
          <ng-container matColumnDef="type">
            <th mat-header-cell *matHeaderCellDef>Type</th>
            <td mat-cell *matCellDef="let a">{{a.accountType}}</td>
          </ng-container>
          <ng-container matColumnDef="balance">
            <th mat-header-cell *matHeaderCellDef>Balance</th>
            <td mat-cell *matCellDef="let a">{{a.balance | currency}}</td>
          </ng-container>
          <ng-container matColumnDef="currency">
            <th mat-header-cell *matHeaderCellDef>Currency</th>
            <td mat-cell *matCellDef="let a">{{a.currency}}</td>
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let a">
              <span class="status-badge" [class.active]="a.isActive">{{a.isActive ? 'Active' : 'Inactive'}}</span>
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
    .accounts-table { width: 100%; }
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
export class AccountsComponent implements OnInit {
  accounts: Account[] = [];
  displayedColumns = ['number', 'customer', 'type', 'balance', 'currency', 'status'];
  totalCount = 0;
  pageSize = 10;
  currentPage = 1;
  searchTerm = '';

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    this.apiService.getAccounts({ page: this.currentPage, pageSize: this.pageSize, search: this.searchTerm }).subscribe({
      next: (res) => {
        this.accounts = res.items;
        this.totalCount = res.totalCount;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadAccounts();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadAccounts();
  }
}
