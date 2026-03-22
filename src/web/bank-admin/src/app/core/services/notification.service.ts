import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface Notification {
  id: number;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  link?: string;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private apiUrl = 'https://localhost:7000/api/notifications';
  
  notifications = signal<Notification[]>([]);
  unreadCount = signal<number>(0);
  
  private pollInterval: any;

  constructor(private http: HttpClient) {}

  loadNotifications(): void {
    this.http.get<any>(this.apiUrl).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.notifications.set(res.data);
          this.unreadCount.set(res.data.filter((n: Notification) => !n.isRead).length);
        }
      }
    });
  }

  getUnreadCount(): void {
    this.http.get<any>(`${this.apiUrl}/unread-count`).subscribe({
      next: (res) => {
        if (res.success && res.data !== undefined) {
          this.unreadCount.set(res.data);
        }
      }
    });
  }

  markAsRead(id: number): void {
    this.http.post<any>(`${this.apiUrl}/${id}/read`, {}).subscribe({
      next: () => {
        this.loadNotifications();
      }
    });
  }

  markAllAsRead(): void {
    this.http.post<any>(`${this.apiUrl}/mark-all-read`, {}).subscribe({
      next: () => {
        this.loadNotifications();
      }
    });
  }

  startPolling(intervalMs: number = 30000): void {
    this.loadNotifications();
    this.pollInterval = setInterval(() => {
      this.getUnreadCount();
    }, intervalMs);
  }

  stopPolling(): void {
    if (this.pollInterval) {
      clearInterval(this.pollInterval);
    }
  }
}
