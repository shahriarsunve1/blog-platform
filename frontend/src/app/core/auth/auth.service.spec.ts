import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('isLoggedIn() is false with no stored token', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('login() stores tokens and current user on success', () => {
    const mockUser = {
      id: '1', username: '', email: 'a@example.com', firstName: 'A', lastName: 'B',
      bio: '', avatar: '', role: 'User', createdAt: ''
    };

    service.login({ email: 'a@example.com', password: 'password123' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    req.flush({
      success: true,
      statusCode: 200,
      message: '',
      data: { accessToken: 'access-token', refreshToken: 'refresh-token', expiresIn: 3600, user: mockUser },
      errors: []
    });

    expect(localStorage.getItem('accessToken')).toBe('access-token');
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.getCurrentUser()?.email).toBe('a@example.com');
  });

  it('logout() clears tokens and current user (regression: used to leave stale currentUser)', () => {
    localStorage.setItem('accessToken', 'x');
    localStorage.setItem('refreshToken', 'y');
    localStorage.setItem('currentUser', JSON.stringify({ email: 'a@example.com' }));

    service.logout();

    expect(localStorage.getItem('accessToken')).toBeNull();
    expect(localStorage.getItem('refreshToken')).toBeNull();
    expect(localStorage.getItem('currentUser')).toBeNull();
    expect(service.getCurrentUser()).toBeNull();
  });
});
