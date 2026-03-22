import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PaginatedRequest {
  page?: number;
  pageSize?: number;
  search?: string;
  startDate?: string;
  endDate?: string;
  type?: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

export interface DashboardSummary {
  totalCustomers: number;
  totalAccounts: number;
  totalBalance: number;
  todayDeposits: number;
  todayWithdrawals: number;
  todayTransactions: number;
  monthlyRevenue: number;
}

export interface Customer {
  customerId: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address?: string;
  city?: string;
  country: string;
  dateOfBirth?: string;
  isActive: boolean;
  createdAt: string;
  accounts?: Account[];
}

export interface Account {
  accountId: number;
  accountNumber: string;
  customerId: number;
  customerName: string;
  accountType: string;
  currency: string;
  balance: number;
  isActive: boolean;
  createdAt: string;
}

export interface Transaction {
  transactionId: number;
  type: string;
  fromAccountId?: number;
  fromAccountNumber?: string;
  toAccountId?: number;
  toAccountNumber?: string;
  amount: number;
  balanceAfter: number;
  referenceNumber: string;
  description?: string;
  status: string;
  createdByName: string;
  createdAt: string;
}

export interface User {
  userId: number;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  lastLoginAt?: string;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private apiUrl = 'https://localhost:7000/api';

  constructor(private http: HttpClient) {}

  private getParams(request: PaginatedRequest): HttpParams {
    let params = new HttpParams();
    if (request.page) params = params.set('page', request.page.toString());
    if (request.pageSize) params = params.set('pageSize', request.pageSize.toString());
    if (request.search) params = params.set('search', request.search);
    if (request.startDate) params = params.set('startDate', request.startDate);
    if (request.endDate) params = params.set('endDate', request.endDate);
    if (request.type) params = params.set('type', request.type);
    return params;
  }

  getDashboardSummary(): Observable<ApiResponse<DashboardSummary>> {
    return this.http.get<ApiResponse<DashboardSummary>>(`${this.apiUrl}/dashboard/summary`);
  }

  getChartData(days: number = 30): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/dashboard/chart-data?days=${days}`);
  }

  getCustomers(request: PaginatedRequest): Observable<PaginatedResponse<Customer>> {
    return this.http.get<PaginatedResponse<Customer>>(`${this.apiUrl}/customers`, { params: this.getParams(request) });
  }

  getCustomer(id: number): Observable<ApiResponse<Customer>> {
    return this.http.get<ApiResponse<Customer>>(`${this.apiUrl}/customers/${id}`);
  }

  createCustomer(customer: Partial<Customer>): Observable<ApiResponse<Customer>> {
    return this.http.post<ApiResponse<Customer>>(`${this.apiUrl}/customers`, customer);
  }

  updateCustomer(id: number, customer: Partial<Customer>): Observable<ApiResponse<Customer>> {
    return this.http.put<ApiResponse<Customer>>(`${this.apiUrl}/customers/${id}`, customer);
  }

  deleteCustomer(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/customers/${id}`);
  }

  getAccounts(request: PaginatedRequest): Observable<PaginatedResponse<Account>> {
    return this.http.get<PaginatedResponse<Account>>(`${this.apiUrl}/accounts`, { params: this.getParams(request) });
  }

  getAccount(id: number): Observable<ApiResponse<Account>> {
    return this.http.get<ApiResponse<Account>>(`${this.apiUrl}/accounts/${id}`);
  }

  createAccount(account: Partial<Account>): Observable<ApiResponse<Account>> {
    return this.http.post<ApiResponse<Account>>(`${this.apiUrl}/accounts`, account);
  }

  getTransactions(request: PaginatedRequest): Observable<PaginatedResponse<Transaction>> {
    return this.http.get<PaginatedResponse<Transaction>>(`${this.apiUrl}/transactions`, { params: this.getParams(request) });
  }

  getRecentTransactions(count: number = 10): Observable<ApiResponse<Transaction[]>> {
    return this.http.get<ApiResponse<Transaction[]>>(`${this.apiUrl}/transactions/recent?count=${count}`);
  }

  deposit(data: { accountId: number; amount: number; description?: string }): Observable<ApiResponse<Transaction>> {
    return this.http.post<ApiResponse<Transaction>>(`${this.apiUrl}/transactions/deposit`, data);
  }

  withdraw(data: { accountId: number; amount: number; description?: string }): Observable<ApiResponse<Transaction>> {
    return this.http.post<ApiResponse<Transaction>>(`${this.apiUrl}/transactions/withdraw`, data);
  }

  transfer(data: { fromAccountId: number; toAccountId: number; amount: number; description?: string }): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/transactions/transfer`, data);
  }

  getUsers(request: PaginatedRequest): Observable<PaginatedResponse<User>> {
    return this.http.get<PaginatedResponse<User>>(`${this.apiUrl}/users`, { params: this.getParams(request) });
  }

  createUser(user: Partial<User>): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(`${this.apiUrl}/users`, user);
  }

  updateUserRole(id: number, role: string): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/users/${id}/role`, { role });
  }

  deleteUser(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/users/${id}`);
  }
}
