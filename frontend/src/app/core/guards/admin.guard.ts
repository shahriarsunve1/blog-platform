import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { AuthService } from '../auth/auth.service';

/**
 * Guard to restrict routes to Admin-role users. Assumes AuthGuard already
 * confirmed the user is logged in.
 */
@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  canActivate(): boolean | UrlTree {
    if (this.authService.getCurrentUser()?.role === 'Admin') {
      return true;
    }

    return this.router.createUrlTree(['/posts']);
  }
}
