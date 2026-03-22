import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { ApiService, Transaction } from '../../core/services/api.service';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule, MatSelectModule, FormsModule],
  template: `
    <h1>Transactions</h1>
    
    <mat-card>
      <mat-card-content>
        <div class="filters">
          <mat-form-field appearance="outline">
            <mat-label>Type</mat-label>
            <mat-select [(ngModel)]="typeFilter" (selectionChange)="onFilterChange()">
              <mat-option value="">All</mat-option>
              <mat-option value="Deposit">Deposit</mat-option>
              <mat-option value="Withdrawal">Withdrawal</mat-option>
              <mat-option value="Transfer">Transfer</mat-option>
            </mat-select>
          </mat-form-field>
        </div>
        
        <table mat-table [dataSource]="transactions" class="transactions-table">
          <ng-container matColumnDef="date">
            <th mat-header-cell *matHeaderCellDef>Date</th>
            <td mat-cell *matCellDef="let t">{{t.createdAt | date:'short'}}</td>
          </ng-container>
          <ng-container matColumnDef="type">
            <th mat-header-cell *matHeaderCellDef>Type</th>
            <td mat-cell *matCellDef="let t">
              <span class="type-badge" [class]="t.type.toLowerCase()">{{t.type}}</span>
            </td>
          </ng-container>
          <ng-container matColumnDef="from">
            <th mat-header-cell *matHeaderCellDef>From</th>
            <td mat-cell *matCellDef="let t">{{t.fromAccountNumber || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="to">
            <th mat-header-cell *matHeaderCellDef>To</th>
            <td mat-cell *matCellDef="let t">{{t.toAccountNumber || '-'}}</td>
          </ng-container>
          <ng-container matColumnDef="amount">
            <th mat-header-cell *matHeaderCellDef>Amount</th>
            <td mat-cell *matCellDef="let t">{{t.amount | currency}}</td>
          </ng-container>
          <ng-container matColumnDef="reference">
            <th mat-header-cell *matHeaderCellDef>Reference</th>
            <td mat-cell *matCellDef="let t">{{t.referenceNumber}}</td>
          </ng-container>
          <ng-container matColumnDef="description">
            <th mat-header-cell *matHeaderCellDef>Description</th>
            <td mat-cell *matCellDef="let t">{{t.description || '-'}}</td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
        
        <mat-paginator [length]="totalCount" [pageSize]="pageSize" [pageSizeOptions]="[10, 25, 50]"
          (page)="onPageChange($event)"></mat-paginator>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .filters { display: flex; gap: 16px; margin-bottom: 20px; }
    .transactions-table { width: 100%; }
    .type-badge {
      padding: 4px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: bold;
    }
    .type-badge.deposit { background: #e8f5e9; color: #4caf50; }
    .type-badge.withdrawal { background: #ffebee; color: #f44336; }
    .type-badge.transfer { background: #e3f2fd; color: #2196f3; }
  `]
})
export class TransactionsComponent implements OnInit {
  transactions: Transaction[] = [];
  displayedColumns = ['date', 'type', 'from', 'to', 'amount', 'reference', 'description'];
  totalCount = 0;
  pageSize = 10;
  currentPage = 1;
  typeFilter = '';

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.apiService.getTransactions({ page: this.currentPage, pageSize: this.pageSize, type: this.typeFilter }).subscribe({
      next: (res) => {
        this.transactions = res.items;
        this.totalCount = res.totalCount;
      }
    });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadTransactions();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadTransactions();
  }
}
