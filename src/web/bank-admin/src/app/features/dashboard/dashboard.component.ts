import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { GoogleChartsModule } from 'angular-google-charts';
import { ApiService, DashboardSummary, Transaction } from '../../core/services/api.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatTableModule, GoogleChartsModule],
  template: `
    <h1>Dashboard</h1>
    
    <div class="stats-grid">
      <mat-card class="stat-card">
        <mat-card-content>
          <mat-icon class="stat-icon blue">people</mat-icon>
          <div class="stat-info">
            <div class="stat-value">{{summary?.totalCustomers || 0}}</div>
            <div class="stat-label">Total Customers</div>
          </div>
        </mat-card-content>
      </mat-card>
      
      <mat-card class="stat-card">
        <mat-card-content>
          <mat-icon class="stat-icon green">account_balance_wallet</mat-icon>
          <div class="stat-info">
            <div class="stat-value">{{summary?.totalAccounts || 0}}</div>
            <div class="stat-label">Total Accounts</div>
          </div>
        </mat-card-content>
      </mat-card>
      
      <mat-card class="stat-card">
        <mat-card-content>
          <mat-icon class="stat-icon purple">attach_money</mat-icon>
          <div class="stat-info">
            <div class="stat-value">{{summary?.totalBalance | currency}}</div>
            <div class="stat-label">Total Balance</div>
          </div>
        </mat-card-content>
      </mat-card>
      
      <mat-card class="stat-card">
        <mat-card-content>
          <mat-icon class="stat-icon orange">swap_horiz</mat-icon>
          <div class="stat-info">
            <div class="stat-value">{{summary?.todayTransactions || 0}}</div>
            <div class="stat-label">Today's Transactions</div>
          </div>
        </mat-card-content>
      </mat-card>
    </div>
    
    <div class="charts-grid">
      <mat-card>
        <mat-card-header>
          <mat-card-title>Transaction Summary</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <div class="chart-container">
            <google-chart [type]="pieChartType" [data]="accountDistributionData" [options]="pieChartOptions"></google-chart>
          </div>
        </mat-card-content>
      </mat-card>
      
      <mat-card>
        <mat-card-header>
          <mat-card-title>Recent Transactions</mat-card-title>
        </mat-card-header>
        <mat-card-content>
          <table mat-table [dataSource]="recentTransactions" class="recent-table">
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
            <ng-container matColumnDef="amount">
              <th mat-header-cell *matHeaderCellDef>Amount</th>
              <td mat-cell *matCellDef="let t">{{t.amount | currency}}</td>
            </ng-container>
            <ng-container matColumnDef="reference">
              <th mat-header-cell *matHeaderCellDef>Reference</th>
              <td mat-cell *matCellDef="let t">{{t.referenceNumber}}</td>
            </ng-container>
            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
          </table>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 20px;
      margin-bottom: 20px;
    }
    .stat-card mat-card-content {
      display: flex;
      align-items: center;
      padding: 20px;
    }
    .stat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-right: 20px;
    }
    .stat-icon.blue { color: #2196f3; }
    .stat-icon.green { color: #4caf50; }
    .stat-icon.purple { color: #9c27b0; }
    .stat-icon.orange { color: #ff9800; }
    .stat-value {
      font-size: 28px;
      font-weight: bold;
    }
    .stat-label {
      color: #666;
    }
    .charts-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
      gap: 20px;
    }
    .chart-container {
      height: 300px;
    }
    .recent-table {
      width: 100%;
    }
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
export class DashboardComponent implements OnInit {
  summary: DashboardSummary | null = null;
  recentTransactions: Transaction[] = [];
  displayedColumns = ['date', 'type', 'amount', 'reference'];
  
  pieChartType = 'PieChart';
  accountDistributionData = [['Account Type', 'Count']];
  pieChartOptions = { title: 'Account Types', is3D: true, width: 400, height: 300 };

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.apiService.getDashboardSummary().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.summary = res.data;
        }
      }
    });

    this.apiService.getRecentTransactions(5).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.recentTransactions = res.data;
        }
      }
    });

    this.apiService.getChartData().subscribe({
      next: (res) => {
        if (res.success && res.data?.accountTypeDistribution) {
          this.accountDistributionData = [['Account Type', 'Count']];
          res.data.accountTypeDistribution.forEach((acc: any) => {
            this.accountDistributionData.push([acc.type, acc.count]);
          });
        }
      }
    });
  }
}
