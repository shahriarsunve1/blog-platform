import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PostService } from '../services/post.service';
import { AuthService } from '../../../core/auth/auth.service';
import { CreatePostDto, UpdatePostDto } from '../../../shared/models/models';

@Component({
  selector: 'app-post-form',
  templateUrl: './post-form.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule]
})
export class PostFormComponent implements OnInit {
  form!: FormGroup;
  isLoading = false;
  isEditing = false;
  postId: string | null = null;
  errorMessage = '';
  successMessage = '';

  statuses = ['Draft', 'Published', 'Archived'];

  constructor(
    private fb: FormBuilder,
    private postService: PostService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditing = true;
      this.postId = id;
      this.loadPost(this.postId);
    }
  }

  private initializeForm(): void {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      excerpt: ['', [Validators.required, Validators.minLength(10)]],
      content: ['', [Validators.required, Validators.minLength(50)]],
      status: ['Draft', Validators.required],
      categoryIds: [[]],
      tagIds: [[]]
    });
  }

  private loadPost(id: string): void {
    this.isLoading = true;
    this.postService.getPostById(id).subscribe({
      next: (response) => {
        if (response.data) {
          this.form.patchValue({
            title: response.data.title,
            excerpt: response.data.excerpt,
            content: response.data.content,
            status: response.data.status
          });
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading post:', err);
        this.errorMessage = 'Failed to load post';
        this.isLoading = false;
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const user = this.authService.getCurrentUser();
    if (!user) {
      this.errorMessage = 'You must be logged in to create/edit posts';
      this.isLoading = false;
      return;
    }

    if (this.isEditing && this.postId) {
      const updateRequest: UpdatePostDto = this.form.value;
      this.postService.updatePost(this.postId, updateRequest).subscribe({
        next: () => {
          this.successMessage = 'Post updated successfully!';
          setTimeout(() => {
            this.router.navigate(['/posts', this.postId]);
          }, 1500);
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to update post';
          this.isLoading = false;
        }
      });
    } else {
      const createRequest: CreatePostDto = this.form.value;
      this.postService.createPost(createRequest).subscribe({
        next: (response) => {
          this.successMessage = 'Post created successfully!';
          setTimeout(() => {
            this.router.navigate(['/posts', response.data?.id]);
          }, 1500);
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to create post';
          this.isLoading = false;
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/posts']);
  }
}
