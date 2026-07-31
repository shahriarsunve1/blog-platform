import { Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'posts', pathMatch: 'full' },
  {
    path: 'admin',
    loadComponent: () => import('./features/admin/components/admin-dashboard.component').then(m => m.AdminDashboardComponent),
    canActivate: [AuthGuard, AdminGuard]
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/components/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/components/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'posts',
    loadComponent: () => import('./features/posts/components/post-list.component').then(m => m.PostListComponent)
  },
  {
    path: 'posts/create',
    loadComponent: () => import('./features/posts/components/post-form.component').then(m => m.PostFormComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'my-posts',
    loadComponent: () => import('./features/posts/components/my-posts.component').then(m => m.MyPostsComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'profile',
    loadComponent: () => import('./features/users/components/profile-settings.component').then(m => m.ProfileSettingsComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'authors/:id',
    loadComponent: () => import('./features/users/components/author-profile.component').then(m => m.AuthorProfileComponent)
  },
  {
    path: 'posts/:id/edit',
    loadComponent: () => import('./features/posts/components/post-form.component').then(m => m.PostFormComponent),
    canActivate: [AuthGuard]
  },
  {
    path: 'posts/:id',
    loadComponent: () => import('./features/posts/components/post-detail.component').then(m => m.PostDetailComponent)
  },
  { path: '**', redirectTo: 'posts' }
];
