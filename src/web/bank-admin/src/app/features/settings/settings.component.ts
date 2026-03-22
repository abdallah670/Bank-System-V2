import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTableModule } from '@angular/material/table';
import { MatBadgeModule } from '@angular/material/badge';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    CommonModule, FormsModule, MatCardModule, MatTabsModule, MatButtonModule,
    MatIconModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule,
    MatTableModule, MatBadgeModule, MatSnackBarModule
  ],
  template: `
    <h1>Settings</h1>
    
    <mat-tab-group>
      <!-- Profile Tab -->
      <mat-tab label="Profile">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Profile Information</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="profile-info">
              <div class="info-row">
                <label>Username:</label>
                <span>{{user?.username}}</span>
              </div>
              <div class="info-row">
                <label>Email:</label>
                <span>{{user?.email}}</span>
              </div>
              <div class="info-row">
                <label>Name:</label>
                <span>{{user?.firstName}} {{user?.lastName}}</span>
              </div>
              <div class="info-row">
                <label>Role:</label>
                <span class="role-badge">{{user?.role}}</span>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </mat-tab>

      <!-- Notifications Tab -->
      <mat-tab label="Notifications">
        <mat-card>
          <mat-card-header>
            <mat-card-title>
              Notifications 
              <button mat-button color="primary" (click)="markAllRead()">Mark all read</button>
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="notifications-list">
              <div *ngFor="let n of notifications()" 
                   class="notification-item" 
                   [class.unread]="!n.isRead"
                   (click)="markRead(n.id)">
                <mat-icon [class]="getNotificationIcon(n.type)">{{getNotificationIcon(n.type)}}</mat-icon>
                <div class="notification-content">
                  <div class="notification-title">{{n.title}}</div>
                  <div class="notification-message">{{n.message}}</div>
                  <div class="notification-time">{{n.createdAt | date:'short'}}</div>
                </div>
              </div>
              <div *ngIf="notifications().length === 0" class="no-notifications">
                No notifications
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </mat-tab>

      <!-- Security Tab -->
      <mat-tab label="Security">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Two-Factor Authentication</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="two-factor-section">
              <p>Add an extra layer of security to your account.</p>
              
              <div *ngIf="!twoFactorEnabled">
                <p>Status: <span class="status-badge disabled">Disabled</span></p>
                <button mat-raised-button color="primary" (click)="setupTwoFactor()">
                  <mat-icon>security</mat-icon>
                  Enable 2FA
                </button>
              </div>

              <div *ngIf="twoFactorEnabled && !showQRCode">
                <p>Status: <span class="status-badge enabled">Enabled</span></p>
                <button mat-raised-button color="warn" (click)="showDisable2FA = true">
                  <mat-icon>lock</mat-icon>
                  Disable 2FA
                </button>
              </div>

              <div *ngIf="showQRCode" class="qr-setup">
                <h3>Scan QR Code</h3>
                <p>Use an authenticator app like Google Authenticator or Authy to scan:</p>
                <div class="qr-code-box">
                  <code>{{qrCodeUrl}}</code>
                </div>
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Verification Code</mat-label>
                  <input matInput [(ngModel)]="verificationCode" maxlength="6" placeholder="000000">
                </mat-form-field>
                <button mat-raised-button color="primary" (click)="enableTwoFactor()">Verify & Enable</button>
                <button mat-button (click)="showQRCode = false">Cancel</button>
              </div>

              <div *ngIf="showDisable2FA" class="disable-2fa">
                <mat-form-field appearance="outline" class="full-width">
                  <mat-label>Verification Code</mat-label>
                  <input matInput [(ngModel)]="verificationCode" maxlength="6" placeholder="000000">
                </mat-form-field>
                <button mat-raised-button color="warn" (click)="disableTwoFactor()">Disable 2FA</button>
                <button mat-button (click)="showDisable2FA = false">Cancel</button>
              </div>
            </div>
          </mat-card-content>
        </mat-card>
      </mat-tab>

      <!-- Sessions Tab -->
      <mat-tab label="Active Sessions">
        <mat-card>
          <mat-card-header>
            <mat-card-title>
              Active Sessions
              <button mat-button color="primary" (click)="revokeAllSessions()">Revoke All Others</button>
            </mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <table mat-table [dataSource]="sessions" class="sessions-table">
              <ng-container matColumnDef="ip">
                <th mat-header-cell *matHeaderCellDef>IP Address</th>
                <td mat-cell *matCellDef="let s">{{s.ipAddress}}</td>
              </ng-container>
              <ng-container matColumnDef="browser">
                <th mat-header-cell *matHeaderCellDef>Browser</th>
                <td mat-cell *matCellDef="let s">{{s.userAgent | slice:0:50}}...</td>
              </ng-container>
              <ng-container matColumnDef="lastActivity">
                <th mat-header-cell *matHeaderCellDef>Last Activity</th>
                <td mat-cell *matCellDef="let s">{{s.lastActivityAt | date:'short'}}</td>
              </ng-container>
              <ng-container matColumnDef="current">
                <th mat-header-cell *matHeaderCellDef>Status</th>
                <td mat-cell *matCellDef="let s">
                  <span *ngIf="s.isCurrent" class="current-badge">Current</span>
                </td>
              </ng-container>
              <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef>Actions</th>
                <td mat-cell *matCellDef="let s">
                  <button mat-button color="warn" *ngIf="!s.isCurrent" (click)="revokeSession(s.sessionId)">
                    Revoke
                  </button>
                </td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="sessionColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: sessionColumns;"></tr>
            </table>
          </mat-card-content>
        </mat-card>
      </mat-tab>

      <!-- Password Tab -->
      <mat-tab label="Change Password">
        <mat-card>
          <mat-card-header>
            <mat-card-title>Change Password</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <form (ngSubmit)="changePassword()">
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Current Password</mat-label>
                <input matInput type="password" [(ngModel)]="currentPassword" name="currentPassword">
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>New Password</mat-label>
                <input matInput type="password" [(ngModel)]="newPassword" name="newPassword">
              </mat-form-field>
              <mat-form-field appearance="outline" class="full-width">
                <mat-label>Confirm New Password</mat-label>
                <input matInput type="password" [(ngModel)]="confirmPassword" name="confirmPassword">
              </mat-form-field>
              <div class="password-requirements">
                <small>Password must contain: 8+ chars, uppercase, lowercase, number, special char</small>
              </div>
              <button mat-raised-button color="primary" type="submit" [disabled]="changingPassword">
                {{changingPassword ? 'Changing...' : 'Change Password'}}
              </button>
            </form>
          </mat-card-content>
        </mat-card>
      </mat-tab>
    </mat-tab-group>
  `,
  styles: [`
    .profile-info { padding: 20px 0; }
    .info-row { display: flex; margin-bottom: 15px; }
    .info-row label { width: 120px; font-weight: bold; }
    .role-badge { padding: 4px 8px; border-radius: 4px; background: #e3f2fd; color: #1976d2; }
    .notifications-list { max-height: 400px; overflow-y: auto; }
    .notification-item { display: flex; padding: 15px; border-bottom: 1px solid #eee; cursor: pointer; }
    .notification-item:hover { background: #f5f5f5; }
    .notification-item.unread { background: #e3f2fd; }
    .notification-content { margin-left: 15px; flex: 1; }
    .notification-title { font-weight: bold; }
    .notification-message { color: #666; font-size: 14px; }
    .notification-time { font-size: 12px; color: #999; margin-top: 5px; }
    .no-notifications { text-align: center; padding: 40px; color: #999; }
    .qr-setup, .disable-2fa { margin-top: 20px; }
    .qr-code-box { background: #f5f5f5; padding: 20px; margin: 15px 0; font-family: monospace; word-break: break-all; }
    .sessions-table { width: 100%; }
    .current-badge { padding: 4px 8px; border-radius: 4px; background: #e8f5e9; color: #4caf50; }
    .full-width { width: 100%; margin-bottom: 10px; }
    .status-badge { padding: 4px 8px; border-radius: 4px; }
    .status-badge.enabled { background: #e8f5e9; color: #4caf50; }
    .status-badge.disabled { background: #ffebee; color: #f44336; }
    .password-requirements { margin-bottom: 15px; color: #666; }
  `]
})
export class SettingsComponent implements OnInit, OnDestroy {
  user = this.authService.currentUser();
  notifications = this.notificationService.notifications;
  
  twoFactorEnabled = false;
  showQRCode = false;
  showDisable2FA = false;
  qrCodeUrl = '';
  verificationCode = '';
  
  sessions: any[] = [];
  sessionColumns = ['ip', 'browser', 'lastActivity', 'current', 'actions'];
  
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  changingPassword = false;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadTwoFactorStatus();
    this.loadSessions();
    this.notificationService.startPolling();
  }

  ngOnDestroy(): void {
    this.notificationService.stopPolling();
  }

  loadTwoFactorStatus(): void {
    this.http.get<any>('https://localhost:7000/api/twofactor/status').subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.twoFactorEnabled = res.data.isEnabled;
        }
      }
    });
  }

  setupTwoFactor(): void {
    this.http.post<any>('https://localhost:7000/api/twofactor/setup', {}).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.qrCodeUrl = res.data.qrCodeUrl;
          this.showQRCode = true;
        }
      }
    });
  }

  enableTwoFactor(): void {
    this.http.post<any>('https://localhost:7000/api/twofactor/enable', {
      secretKey: this.qrCodeUrl,
      code: this.verificationCode
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Two-factor authentication enabled!', 'Close', { duration: 5000 });
          this.showQRCode = false;
          this.twoFactorEnabled = true;
          this.verificationCode = '';
        } else {
          this.snackBar.open('Invalid code. Please try again.', 'Close', { duration: 5000 });
        }
      }
    });
  }

  disableTwoFactor(): void {
    this.http.post<any>('https://localhost:7000/api/twofactor/disable', {
      code: this.verificationCode
    }).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Two-factor authentication disabled.', 'Close', { duration: 5000 });
          this.showDisable2FA = false;
          this.twoFactorEnabled = false;
          this.verificationCode = '';
        } else {
          this.snackBar.open('Invalid code. Please try again.', 'Close', { duration: 5000 });
        }
      }
    });
  }

  loadSessions(): void {
    this.http.get<any>('https://localhost:7000/api/sessions').subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.sessions = res.data;
        }
      }
    });
  }

  revokeSession(sessionId: string): void {
    this.http.post<any>(`https://localhost:7000/api/sessions/revoke/${sessionId}`, {}).subscribe({
      next: () => {
        this.loadSessions();
        this.snackBar.open('Session revoked.', 'Close', { duration: 3000 });
      }
    });
  }

  revokeAllSessions(): void {
    this.http.post<any>('https://localhost:7000/api/sessions/revoke-all', {}).subscribe({
      next: () => {
        this.loadSessions();
        this.snackBar.open('All other sessions revoked.', 'Close', { duration: 3000 });
      }
    });
  }

  markRead(id: number): void {
    this.notificationService.markAsRead(id);
  }

  markAllRead(): void {
    this.notificationService.markAllAsRead();
  }

  getNotificationIcon(type: string): string {
    const icons: any = {
      'Info': 'info',
      'Success': 'check_circle',
      'Warning': 'warning',
      'Error': 'error',
      'Security': 'security'
    };
    return icons[type] || 'notifications';
  }

  changePassword(): void {
    if (this.newPassword !== this.confirmPassword) {
      this.snackBar.open('Passwords do not match.', 'Close', { duration: 3000 });
      return;
    }

    this.changingPassword = true;
    this.http.post<any>('https://localhost:7000/api/auth/change-password', {
      currentPassword: this.currentPassword,
      newPassword: this.newPassword
    }).subscribe({
      next: (res) => {
        this.changingPassword = false;
        if (res.success) {
          this.snackBar.open('Password changed successfully!', 'Close', { duration: 5000 });
          this.currentPassword = '';
          this.newPassword = '';
          this.confirmPassword = '';
        } else {
          this.snackBar.open(res.message || 'Failed to change password.', 'Close', { duration: 5000 });
        }
      },
      error: () => {
        this.changingPassword = false;
        this.snackBar.open('An error occurred.', 'Close', { duration: 5000 });
      }
    });
  }
}
