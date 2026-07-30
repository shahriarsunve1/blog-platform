import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { User, LoginRequest, RegisterRequest, AuthResponse, ApiResponse } from '../../shared/models/models';

/**
 * Authentication service
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredUser();
  }

  /**
   * Register new user
   */
  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/register`, request).pipe(
      map(response => response.data!),
      tap(data => this.storeAuthData(data))
    );
  }

  /**
   * Login user
   */
  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/login`, request).pipe(
      map(response => response.data!),
      tap(data => this.storeAuthData(data))
    );
  }

  /**
   * Logout user
   */
  logout(): void {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    this.currentUserSubject.next(null);
  }

  /**
   * Check if user is logged in
   */
  isLoggedIn(): boolean {
    return !!localStorage.getItem('accessToken');
  }

  /**
   * Get current user
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Load stored user from localStorage
   */
  private loadStoredUser(): void {
    const storedUser = localStorage.getItem('currentUser');
    if (storedUser) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  /**
   * Store authentication data
   */
  storeAuthData(response: AuthResponse): void {
    localStorage.setItem('accessToken', response.accessToken);
    localStorage.setItem('refreshToken', response.refreshToken);
    localStorage.setItem('currentUser', JSON.stringify(response.user));
    this.currentUserSubject.next(response.user);
  }
}
