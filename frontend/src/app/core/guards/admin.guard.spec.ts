import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { AdminGuard } from './admin.guard';
import { AuthService } from '../auth/auth.service';

describe('AdminGuard', () => {
  let guard: AdminGuard;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;
  let redirectTree: UrlTree;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['getCurrentUser']);
    redirectTree = {} as UrlTree;
    router = jasmine.createSpyObj('Router', ['createUrlTree']);
    router.createUrlTree.and.returnValue(redirectTree);

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    });

    guard = TestBed.inject(AdminGuard);
  });

  it('allows navigation for Admin users', () => {
    authService.getCurrentUser.and.returnValue({ role: 'Admin' } as any);

    expect(guard.canActivate()).toBeTrue();
  });

  it('redirects to /posts for non-admin users', () => {
    authService.getCurrentUser.and.returnValue({ role: 'User' } as any);

    expect(guard.canActivate()).toBe(redirectTree);
    expect(router.createUrlTree).toHaveBeenCalledWith(['/posts']);
  });

  it('redirects to /posts when not logged in', () => {
    authService.getCurrentUser.and.returnValue(null);

    expect(guard.canActivate()).toBe(redirectTree);
  });
});
