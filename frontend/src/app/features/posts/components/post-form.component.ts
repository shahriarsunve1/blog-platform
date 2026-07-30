import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
import { PostService } from '../services/post.service';
import { TaxonomyService } from '../services/taxonomy.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Category, CreatePostDto, Tag, UpdatePostDto } from '../../../shared/models/models';

@Component({
  selector: 'app-post-form',
  templateUrl: './post-form.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule]
})
export class PostFormComponent implements OnInit {
  form!: FormGroup;
  isLoading = false;
  isEditing = false;
  postId: string | null = null;
  errorMessage = '';
  successMessage = '';

  statuses = ['Draft', 'Published', 'Archived'];

  categories: Category[] = [];
  tags: Tag[] = [];
  selectedCategoryIds: string[] = [];
  selectedTagIds: string[] = [];

  newCategoryName = '';
  newTagName = '';
  isAddingCategory = false;
  isAddingTag = false;
  taxonomyError = '';

  constructor(
    private fb: FormBuilder,
    private postService: PostService,
    private taxonomyService: TaxonomyService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeForm();

    forkJoin({
      categories: this.taxonomyService.getCategories(),
      tags: this.taxonomyService.getTags()
    }).subscribe({
      next: ({ categories, tags }) => {
        this.categories = categories.data ?? [];
        this.tags = tags.data ?? [];

        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
          this.isEditing = true;
          this.postId = id;
          this.loadPost(this.postId);
        }
      },
      error: (err) => console.error('Error loading categories/tags:', err)
    });
  }

  private initializeForm(): void {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      excerpt: ['', [Validators.required, Validators.minLength(10)]],
      content: ['', [Validators.required, Validators.minLength(50)]],
      status: ['Draft', Validators.required]
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

          const categoryNames = new Set(response.data.categories);
          this.selectedCategoryIds = this.categories
            .filter(c => categoryNames.has(c.name))
            .map(c => c.id);

          const tagNames = new Set(response.data.tags);
          this.selectedTagIds = this.tags
            .filter(t => tagNames.has(t.name))
            .map(t => t.id);
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

  isCategorySelected(id: string): boolean {
    return this.selectedCategoryIds.includes(id);
  }

  toggleCategory(id: string): void {
    this.selectedCategoryIds = this.isCategorySelected(id)
      ? this.selectedCategoryIds.filter(c => c !== id)
      : [...this.selectedCategoryIds, id];
  }

  isTagSelected(id: string): boolean {
    return this.selectedTagIds.includes(id);
  }

  toggleTag(id: string): void {
    this.selectedTagIds = this.isTagSelected(id)
      ? this.selectedTagIds.filter(t => t !== id)
      : [...this.selectedTagIds, id];
  }

  addCategory(): void {
    const name = this.newCategoryName.trim();
    if (!name) return;

    this.isAddingCategory = true;
    this.taxonomyError = '';
    this.taxonomyService.createCategory({ name }).subscribe({
      next: (response) => {
        if (response.data) {
          this.categories = [...this.categories, response.data].sort((a, b) => a.name.localeCompare(b.name));
          this.selectedCategoryIds = [...this.selectedCategoryIds, response.data.id];
        }
        this.newCategoryName = '';
        this.isAddingCategory = false;
      },
      error: (err) => {
        this.taxonomyError = err.error?.message || 'Failed to add category';
        this.isAddingCategory = false;
      }
    });
  }

  addTag(): void {
    const name = this.newTagName.trim();
    if (!name) return;

    this.isAddingTag = true;
    this.taxonomyError = '';
    this.taxonomyService.createTag({ name }).subscribe({
      next: (response) => {
        if (response.data) {
          this.tags = [...this.tags, response.data].sort((a, b) => a.name.localeCompare(b.name));
          this.selectedTagIds = [...this.selectedTagIds, response.data.id];
        }
        this.newTagName = '';
        this.isAddingTag = false;
      },
      error: (err) => {
        this.taxonomyError = err.error?.message || 'Failed to add tag';
        this.isAddingTag = false;
      }
    });
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

    const payload = {
      ...this.form.value,
      categoryIds: this.selectedCategoryIds,
      tagIds: this.selectedTagIds
    };

    if (this.isEditing && this.postId) {
      const updateRequest: UpdatePostDto = payload;
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
      const createRequest: CreatePostDto = payload;
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
