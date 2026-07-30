import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostService } from '../services/post.service';
import { TaxonomyService } from '../services/taxonomy.service';
import { Category, Post, Tag } from '../../../shared/models/models';
import { PostCardComponent } from './post-card.component';

@Component({
  selector: 'app-post-list',
  templateUrl: './post-list.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, PostCardComponent]
})
export class PostListComponent implements OnInit {
  posts: Post[] = [];
  isLoading = false;
  currentPage = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;
  searchQuery = '';

  categories: Category[] = [];
  tags: Tag[] = [];
  selectedCategoryId = '';
  selectedTagId = '';

  constructor(
    private postService: PostService,
    private taxonomyService: TaxonomyService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.taxonomyService.getCategories().subscribe({
      next: (response) => (this.categories = response.data ?? [])
    });
    this.taxonomyService.getTags().subscribe({
      next: (response) => (this.tags = response.data ?? [])
    });
    this.loadPosts();
  }

  loadPosts(): void {
    this.isLoading = true;
    this.postService
      .getPublishedPosts(this.currentPage, this.pageSize, this.selectedCategoryId || undefined, this.selectedTagId || undefined)
      .subscribe({
        next: (response) => {
          if (response.data) {
            this.posts = response.data.items;
            this.totalCount = response.data.totalCount;
            this.totalPages = response.data.totalPages;
          }
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Error loading posts:', err);
          this.isLoading = false;
        }
      });
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadPosts();
  }

  clearFilters(): void {
    this.selectedCategoryId = '';
    this.selectedTagId = '';
    this.onFilterChange();
  }

  viewPost(postId: string): void {
    this.router.navigate(['/posts', postId]);
  }

  createPost(): void {
    this.router.navigate(['/posts/create']);
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadPosts();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadPosts();
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadPosts();
    }
  }

  get pagesArray(): number[] {
    return Array.from({ length: this.totalPages }, (_, i) => i + 1);
  }

  get hasActiveFilters(): boolean {
    return !!this.selectedCategoryId || !!this.selectedTagId;
  }
}
