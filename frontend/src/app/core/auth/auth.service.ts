import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { User, LoginRequest, RegisterRequest, RegisterResult, AuthResponse, ApiResponse, ForgotPasswordRequest, ResetPasswordRequest } from '../../shared/models/models';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

/**
 * Authentication service
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly isBrowser: boolean;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, @Inject(PLATFORM_ID) platformId: object) {
    this.isBrowser = isPlatformBrowser(platformId);
    this.loadStoredUser();
  }

  /**
   * Register new user. Doesn't log them in - the account stays inactive
   * until they click the verification link emailed to them.
   */
  register(request: RegisterRequest): Observable<RegisterResult> {
    return this.http.post<ApiResponse<RegisterResult>>(`${this.apiUrl}/register`, request).pipe(
      map(response => response.data!)
    );
  }

  /**
   * Confirm an emailed verification link
   */
  verifyEmail(token: string): Observable<void> {
    return this.http.post<ApiResponse<object>>(`${this.apiUrl}/verify-email`, { token }).pipe(
      map(() => undefined)
    );
  }

  /**
   * Request a new verification email
   */
  resendVerification(email: string): Observable<void> {
    return this.http.post<ApiResponse<object>>(`${this.apiUrl}/resend-verification`, { email }).pipe(
      map(() => undefined)
    );
  }

  /**
   * Request a password reset email
   */
  forgotPassword(request: ForgotPasswordRequest): Observable<void> {
    return this.http.post<ApiResponse<object>>(`${this.apiUrl}/forgot-password`, request).pipe(
      map(() => undefined)
    );
  }

  /**
   * Confirm an emailed password reset link with a new password
   */
  resetPassword(request: ResetPasswordRequest): Observable<void> {
    return this.http.post<ApiResponse<object>>(`${this.apiUrl}/reset-password`, request).pipe(
      map(() => undefined)
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
   * Exchange the stored (possibly expired) access token + refresh token for a
   * new pair. Fails (and logs out) if there's no refresh token or it's invalid.
   */
  refreshToken(): Observable<AuthResponse> {
    const accessToken = this.isBrowser ? localStorage.getItem('accessToken') : null;
    const refreshToken = this.isBrowser ? localStorage.getItem('refreshToken') : null;

    if (!accessToken || !refreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<ApiResponse<AuthResponse>>(`${this.apiUrl}/refresh`, { accessToken, refreshToken }).pipe(
      map(response => response.data!),
      tap(data => this.storeAuthData(data)),
      catchError(err => {
        this.logout();
        return throwError(() => err);
      })
    );
  }

  /**
   * Logout user
   */
  logout(): void {
    if (this.isBrowser) {
      localStorage.removeItem('accessToken');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('currentUser');
    }
    this.currentUserSubject.next(null);
  }

  /**
   * Check if user is logged in
   */
  isLoggedIn(): boolean {
    return this.isBrowser && !!localStorage.getItem('accessToken');
  }

  /**
   * Get current user
   */
  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  /**
   * Refresh the cached current-user snapshot (e.g. after a profile edit)
   */
  updateCurrentUser(user: User): void {
    if (this.isBrowser) {
      localStorage.setItem('currentUser', JSON.stringify(user));
    }
    this.currentUserSubject.next(user);
  }

  /**
   * Load stored user from localStorage
   */
  private loadStoredUser(): void {
    if (!this.isBrowser) return;

    const storedUser = localStorage.getItem('currentUser');
    if (storedUser && this.isLoggedIn()) {
      this.currentUserSubject.next(JSON.parse(storedUser));
    }
  }

  /**
   * Store authentication data
   */
  storeAuthData(response: AuthResponse): void {
    if (this.isBrowser) {
      localStorage.setItem('accessToken', response.accessToken);
      localStorage.setItem('refreshToken', response.refreshToken);
      localStorage.setItem('currentUser', JSON.stringify(response.user));
    }
    this.currentUserSubject.next(response.user);
  }
}
