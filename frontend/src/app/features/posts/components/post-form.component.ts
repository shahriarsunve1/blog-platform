import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { QuillModule } from 'ngx-quill';
import { forkJoin } from 'rxjs';
import { PostService } from '../services/post.service';
import { TaxonomyService } from '../services/taxonomy.service';
import { MediaService } from '../services/media.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Category, CreatePostDto, Tag, UpdatePostDto } from '../../../shared/models/models';

const ALLOWED_IMAGE_TYPES = ['image/png', 'image/jpeg', 'image/gif', 'image/webp'];
const MAX_IMAGE_SIZE_BYTES = 5 * 1024 * 1024;
const AUTOSAVE_INTERVAL_MS = 10000;

interface PostDraft {
  title: string;
  excerpt: string;
  content: string;
  status: string;
  coverImageUrl: string;
  selectedCategoryIds: string[];
  selectedTagIds: string[];
  savedAt: string;
}

@Component({
  selector: 'app-post-form',
  templateUrl: './post-form.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule, QuillModule]
})
export class PostFormComponent implements OnInit, OnDestroy {
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

  coverImageUrl = '';
  isUploadingCoverImage = false;

  draftBannerVisible = false;
  draftSavedAt: Date | null = null;
  private pendingDraft: PostDraft | null = null;
  private autosaveIntervalId: ReturnType<typeof setInterval> | null = null;

  private quillEditor: any;

  quillModules = {
    toolbar: {
      container: [
        [{ header: [1, 2, 3, false] }],
        ['bold', 'italic', 'underline', 'strike'],
        ['blockquote', 'code-block'],
        [{ list: 'ordered' }, { list: 'bullet' }],
        ['link', 'image'],
        ['clean']
      ],
      handlers: {
        image: () => this.selectAndUploadImage()
      }
    }
  };

  constructor(
    private fb: FormBuilder,
    private postService: PostService,
    private taxonomyService: TaxonomyService,
    private mediaService: MediaService,
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
        } else {
          this.checkForDraft();
        }
      },
      error: (err) => console.error('Error loading categories/tags:', err)
    });

    this.autosaveIntervalId = setInterval(() => this.saveDraftSnapshot(), AUTOSAVE_INTERVAL_MS);
  }

  ngOnDestroy(): void {
    if (this.autosaveIntervalId) {
      clearInterval(this.autosaveIntervalId);
    }
  }

  private get draftStorageKey(): string {
    return `post-draft:${this.postId ?? 'new'}`;
  }

  private checkForDraft(): void {
    const raw = localStorage.getItem(this.draftStorageKey);
    if (!raw) return;

    try {
      const draft = JSON.parse(raw) as PostDraft;
      this.pendingDraft = draft;
      this.draftSavedAt = new Date(draft.savedAt);
      this.draftBannerVisible = true;
    } catch {
      localStorage.removeItem(this.draftStorageKey);
    }
  }

  private saveDraftSnapshot(): void {
    const title = (this.form.get('title')?.value ?? '').trim();
    const content = (this.form.get('content')?.value ?? '').trim();
    if (!title && !content) return;

    const snapshot: PostDraft = {
      title: this.form.get('title')?.value ?? '',
      excerpt: this.form.get('excerpt')?.value ?? '',
      content: this.form.get('content')?.value ?? '',
      status: this.form.get('status')?.value ?? 'Draft',
      coverImageUrl: this.coverImageUrl,
      selectedCategoryIds: this.selectedCategoryIds,
      selectedTagIds: this.selectedTagIds,
      savedAt: new Date().toISOString()
    };
    localStorage.setItem(this.draftStorageKey, JSON.stringify(snapshot));
  }

  restoreDraft(): void {
    if (!this.pendingDraft) return;

    this.form.patchValue({
      title: this.pendingDraft.title,
      excerpt: this.pendingDraft.excerpt,
      content: this.pendingDraft.content,
      status: this.pendingDraft.status
    });
    this.coverImageUrl = this.pendingDraft.coverImageUrl;
    this.selectedCategoryIds = this.pendingDraft.selectedCategoryIds;
    this.selectedTagIds = this.pendingDraft.selectedTagIds;

    this.draftBannerVisible = false;
    this.pendingDraft = null;
  }

  discardDraft(): void {
    localStorage.removeItem(this.draftStorageKey);
    this.draftBannerVisible = false;
    this.pendingDraft = null;
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
          this.coverImageUrl = response.data.coverImageUrl;

          const categoryNames = new Set(response.data.categories);
          this.selectedCategoryIds = this.categories
            .filter(c => categoryNames.has(c.name))
            .map(c => c.id);

          const tagNames = new Set(response.data.tags);
          this.selectedTagIds = this.tags
            .filter(t => tagNames.has(t.name))
            .map(t => t.id);

          this.checkForDraft();
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

  onEditorCreated(editor: any): void {
    this.quillEditor = editor;
  }

  onCoverImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
      this.errorMessage = 'Only PNG, JPEG, GIF, and WEBP images are supported';
      input.value = '';
      return;
    }
    if (file.size > MAX_IMAGE_SIZE_BYTES) {
      this.errorMessage = 'Image exceeds the 5MB size limit';
      input.value = '';
      return;
    }

    this.errorMessage = '';
    this.isUploadingCoverImage = true;
    this.mediaService.upload(file).subscribe({
      next: (response) => {
        if (response.data) {
          this.coverImageUrl = response.data.url;
        }
        this.isUploadingCoverImage = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to upload image';
        this.isUploadingCoverImage = false;
      }
    });

    input.value = '';
  }

  removeCoverImage(): void {
    this.coverImageUrl = '';
  }

  private selectAndUploadImage(): void {
    const input = document.createElement('input');
    input.setAttribute('type', 'file');
    input.setAttribute('accept', ALLOWED_IMAGE_TYPES.join(','));

    input.onchange = () => {
      const file = input.files?.[0];
      if (!file) return;

      if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
        this.errorMessage = 'Only PNG, JPEG, GIF, and WEBP images are supported';
        return;
      }
      if (file.size > MAX_IMAGE_SIZE_BYTES) {
        this.errorMessage = 'Image exceeds the 5MB size limit';
        return;
      }

      const range = this.quillEditor.getSelection(true);
      this.mediaService.upload(file).subscribe({
        next: (response) => {
          if (response.data) {
            this.quillEditor.insertEmbed(range.index, 'image', response.data.url, 'user');
            this.quillEditor.setSelection(range.index + 1, 0);
          }
        },
        error: (err) => {
          this.errorMessage = err.error?.message || 'Failed to upload image';
        }
      });
    };

    input.click();
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
      coverImageUrl: this.coverImageUrl,
      categoryIds: this.selectedCategoryIds,
      tagIds: this.selectedTagIds
    };

    if (this.isEditing && this.postId) {
      const updateRequest: UpdatePostDto = payload;
      this.postService.updatePost(this.postId, updateRequest).subscribe({
        next: () => {
          localStorage.removeItem(this.draftStorageKey);
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
          localStorage.removeItem(this.draftStorageKey);
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
