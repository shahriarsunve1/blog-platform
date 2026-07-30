# Frontend Structure - Angular

## Project Setup

### Directory Structure

```
blog-frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── auth.service.spec.ts
│   │   │   │   ├── jwt.guard.ts
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   ├── auth.models.ts
│   │   │   │   └── index.ts
│   │   │   ├── guards/
│   │   │   │   ├── auth.guard.ts
│   │   │   │   ├── admin.guard.ts
│   │   │   │   └── index.ts
│   │   │   ├── http/
│   │   │   │   ├── api.service.ts
│   │   │   │   ├── http.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── services/
│   │   │   │   ├── notification.service.ts
│   │   │   │   ├── loader.service.ts
│   │   │   │   └── index.ts
│   │   │   ├── core.module.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── shared/
│   │   │   ├── components/
│   │   │   │   ├── header/
│   │   │   │   │   ├── header.component.ts
│   │   │   │   │   ├── header.component.html
│   │   │   │   │   ├── header.component.scss
│   │   │   │   │   └── header.component.spec.ts
│   │   │   │   ├── footer/
│   │   │   │   ├── loader-spinner/
│   │   │   │   └── index.ts
│   │   │   ├── directives/
│   │   │   │   ├── debounce-click.directive.ts
│   │   │   │   ├── debounce-click.directive.spec.ts
│   │   │   │   └── index.ts
│   │   │   ├── pipes/
│   │   │   │   ├── time-ago.pipe.ts
│   │   │   │   ├── time-ago.pipe.spec.ts
│   │   │   │   └── index.ts
│   │   │   ├── models/
│   │   │   │   ├── common.models.ts
│   │   │   │   ├── error.models.ts
│   │   │   │   └── index.ts
│   │   │   ├── shared.module.ts
│   │   │   └── index.ts
│   │   │
│   │   ├── features/
│   │   │   ├── posts/
│   │   │   │   ├── components/
│   │   │   │   │   ├── post-list/
│   │   │   │   │   │   ├── post-list.component.ts
│   │   │   │   │   │   ├── post-list.component.html
│   │   │   │   │   │   ├── post-list.component.scss
│   │   │   │   │   │   └── post-list.component.spec.ts
│   │   │   │   │   ├── post-detail/
│   │   │   │   │   ├── post-form/
│   │   │   │   │   ├── post-card/
│   │   │   │   │   └── index.ts
│   │   │   │   ├── services/
│   │   │   │   │   ├── post.service.ts
│   │   │   │   │   ├── post.service.spec.ts
│   │   │   │   │   └── index.ts
│   │   │   │   ├── models/
│   │   │   │   │   ├── post.models.ts
│   │   │   │   │   └── index.ts
│   │   │   │   ├── pages/
│   │   │   │   │   ├── posts-page/
│   │   │   │   │   ├── post-detail-page/
│   │   │   │   │   └── create-post-page/
│   │   │   │   ├── posts.module.ts
│   │   │   │   ├── posts-routing.module.ts
│   │   │   │   └── index.ts
│   │   │   │
│   │   │   ├── auth/
│   │   │   │   ├── components/
│   │   │   │   │   ├── login/
│   │   │   │   │   ├── register/
│   │   │   │   │   └── index.ts
│   │   │   │   ├── auth.module.ts
│   │   │   │   ├── auth-routing.module.ts
│   │   │   │   └── index.ts
│   │   │   │
│   │   │   └── user/
│   │   │       ├── components/
│   │   │       ├── services/
│   │   │       ├── user.module.ts
│   │   │       ├── user-routing.module.ts
│   │   │       └── index.ts
│   │   │
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app.component.scss
│   │   ├── app.component.spec.ts
│   │   ├── app.module.ts
│   │   ├── app-routing.module.ts
│   │   └── app.config.ts
│   │
│   ├── assets/
│   │   ├── images/
│   │   ├── icons/
│   │   └── styles/
│   │       ├── _variables.scss
│   │       ├── _mixins.scss
│   │       ├── _animations.scss
│   │       └── global.scss
│   │
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   │
│   ├── index.html
│   ├── main.ts
│   └── styles.scss
│
├── angular.json
├── tsconfig.json
├── tsconfig.app.json
├── tsconfig.spec.json
├── karma.conf.js
├── package.json
├── package-lock.json
└── README.md
```

## Module Organization

### Core Module

**Purpose**: Singleton services that should exist only once in the application

```typescript
// core/core.module.ts
import { NgModule, Optional, SkipSelf } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthService } from './auth/auth.service';
import { AuthInterceptor } from './auth/auth.interceptor';
import { ErrorInterceptor } from './http/error.interceptor';

@NgModule({
  providers: [
    AuthService,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ]
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error('CoreModule is already loaded. Import only once in AppModule');
    }
  }
}
```

### Shared Module

**Purpose**: Reusable components, directives, pipes

```typescript
// shared/shared.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './components/header/header.component';
import { FooterComponent } from './components/footer/footer.component';
import { LoaderSpinnerComponent } from './components/loader-spinner/loader-spinner.component';
import { TimeAgoPipe } from './pipes/time-ago.pipe';
import { DebounceClickDirective } from './directives/debounce-click.directive';

const COMPONENTS = [
  HeaderComponent,
  FooterComponent,
  LoaderSpinnerComponent
];

const DIRECTIVES = [DebounceClickDirective];

const PIPES = [TimeAgoPipe];

@NgModule({
  imports: [CommonModule],
  declarations: [...COMPONENTS, ...DIRECTIVES, ...PIPES],
  exports: [...COMPONENTS, ...DIRECTIVES, ...PIPES]
})
export class SharedModule { }
```

### Feature Modules (Lazy Loading)

```typescript
// features/posts/posts.module.ts
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PostsRoutingModule } from './posts-routing.module';
import { PostListComponent } from './components/post-list/post-list.component';
import { PostDetailComponent } from './components/post-detail/post-detail.component';
import { PostFormComponent } from './components/post-form/post-form.component';
import { PostService } from './services/post.service';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [
    PostListComponent,
    PostDetailComponent,
    PostFormComponent
  ],
  imports: [
    CommonModule,
    PostsRoutingModule,
    SharedModule
  ],
  providers: [PostService]
})
export class PostsModule { }
```

## Service Architecture

### API Service Pattern

```typescript
// core/http/api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  get<T>(endpoint: string, options = {}): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${endpoint}`, options);
  }

  post<T>(endpoint: string, data: any, options = {}): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${endpoint}`, data, options);
  }

  put<T>(endpoint: string, data: any, options = {}): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${endpoint}`, data, options);
  }

  delete<T>(endpoint: string, options = {}): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${endpoint}`, options);
  }
}
```

### Feature Service Pattern

```typescript
// features/posts/services/post.service.ts
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/http/api.service';
import { Post, CreatePostDto, UpdatePostDto } from '../models/post.models';

@Injectable({
  providedIn: 'root'
})
export class PostService {
  private readonly endpoint = '/posts';

  constructor(private api: ApiService) { }

  getPosts(page: number = 1, pageSize: number = 10): Observable<any> {
    return this.api.get(`${this.endpoint}`, {
      params: { page, pageSize }
    });
  }

  getPostById(id: string): Observable<Post> {
    return this.api.get(`${this.endpoint}/${id}`);
  }

  createPost(dto: CreatePostDto): Observable<Post> {
    return this.api.post(this.endpoint, dto);
  }

  updatePost(id: string, dto: UpdatePostDto): Observable<Post> {
    return this.api.put(`${this.endpoint}/${id}`, dto);
  }

  deletePost(id: string): Observable<void> {
    return this.api.delete(`${this.endpoint}/${id}`);
  }

  getUserPosts(userId: string): Observable<Post[]> {
    return this.api.get(`${this.endpoint}/user/${userId}`);
  }
}
```

## Component Patterns

### Smart Component (Container)

```typescript
// features/posts/pages/posts-page/posts-page.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PostService } from '../../services/post.service';
import { Post } from '../../models/post.models';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-posts-page',
  templateUrl: './posts-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostsPageComponent implements OnInit, OnDestroy {
  posts$ = this.postService.getPosts();
  loading$ = this.postService.loading$;
  error$ = this.postService.error$;

  private destroy$ = new Subject<void>();

  constructor(
    private postService: PostService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPosts();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadPosts(): void {
    this.postService.getPosts()
      .pipe(takeUntil(this.destroy$))
      .subscribe(
        posts => console.log('Posts loaded', posts),
        error => console.error('Error loading posts', error)
      );
  }

  onViewPost(postId: string): void {
    this.router.navigate(['/posts', postId]);
  }
}
```

### Dumb Component (Presentational)

```typescript
// features/posts/components/post-card/post-card.component.ts
import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { Post } from '../../models/post.models';

@Component({
  selector: 'app-post-card',
  templateUrl: './post-card.component.html',
  styleUrls: ['./post-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostCardComponent {
  @Input() post: Post | null = null;
  @Output() viewPost = new EventEmitter<string>();
  @Output() editPost = new EventEmitter<string>();

  onViewClick(): void {
    if (this.post?.id) {
      this.viewPost.emit(this.post.id);
    }
  }

  onEditClick(): void {
    if (this.post?.id) {
      this.editPost.emit(this.post.id);
    }
  }
}
```

## Reactive Forms Pattern

```typescript
// features/posts/components/post-form/post-form.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PostService } from '../../services/post.service';

@Component({
  selector: 'app-post-form',
  templateUrl: './post-form.component.html',
  styleUrls: ['./post-form.component.scss']
})
export class PostFormComponent implements OnInit {
  form!: FormGroup;
  submitted = false;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private postService: PostService
  ) { }

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(5)]],
      excerpt: ['', [Validators.required, Validators.maxLength(200)]],
      content: ['', [Validators.required, Validators.minLength(50)]],
      status: ['Draft', Validators.required],
      categories: [[]],
      tags: [[]]
    });
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.form.invalid) {
      return;
    }

    this.loading = true;
    this.postService.createPost(this.form.value)
      .subscribe(
        () => {
          this.loading = false;
          // Handle success
        },
        error => {
          this.loading = false;
          // Handle error
        }
      );
  }

  get titleError(): string | null {
    const control = this.form.get('title');
    if (control?.hasError('required')) return 'Title is required';
    if (control?.hasError('minlength')) return 'Title must be at least 5 characters';
    return null;
  }
}
```

## Route Guards

```typescript
// core/guards/auth.guard.ts
import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  canActivate(): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    if (this.authService.isLoggedIn()) {
      return true;
    }

    this.router.navigate(['/auth/login'], {
      queryParams: { returnUrl: this.router.url }
    });
    return false;
  }
}
```

## Routing Configuration

```typescript
// app-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: '',
        redirectTo: '/posts',
        pathMatch: 'full'
      },
      {
        path: 'posts',
        loadChildren: () => import('./features/posts/posts.module')
          .then(m => m.PostsModule)
      },
      {
        path: 'auth',
        loadChildren: () => import('./features/auth/auth.module')
          .then(m => m.AuthModule)
      },
      {
        path: 'user',
        loadChildren: () => import('./features/user/user.module')
          .then(m => m.UserModule),
        canActivate: [AuthGuard]
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
```

## Styles Organization

```scss
// assets/styles/_variables.scss
$primary-color: #007bff;
$secondary-color: #6c757d;
$success-color: #28a745;
$error-color: #dc3545;

$font-family-base: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
$font-size-base: 1rem;
$line-height-base: 1.5;

$spacing-unit: 1rem;
$spacing-xs: $spacing-unit * 0.25;
$spacing-sm: $spacing-unit * 0.5;
$spacing-md: $spacing-unit;
$spacing-lg: $spacing-unit * 2;

// assets/styles/_mixins.scss
@mixin flex-center {
  display: flex;
  justify-content: center;
  align-items: center;
}

@mixin card-shadow {
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.12),
              0 1px 2px rgba(0, 0, 0, 0.24);
}
```

---

**Last Updated**: July 2026
