import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/components/login.component';
import { RegisterComponent } from './features/auth/components/register.component';
import { PostListComponent } from './features/posts/components/post-list.component';
import { PostDetailComponent } from './features/posts/components/post-detail.component';
import { PostFormComponent } from './features/posts/components/post-form.component';
import { MyPostsComponent } from './features/posts/components/my-posts.component';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'posts', pathMatch: 'full' },
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },
  { path: 'posts', component: PostListComponent },
  { path: 'posts/create', component: PostFormComponent, canActivate: [AuthGuard] },
  { path: 'my-posts', component: MyPostsComponent, canActivate: [AuthGuard] },
  { path: 'posts/:id/edit', component: PostFormComponent, canActivate: [AuthGuard] },
  { path: 'posts/:id', component: PostDetailComponent },
  { path: '**', redirectTo: 'posts' }
];
