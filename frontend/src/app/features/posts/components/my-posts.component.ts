import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { PostService } from '../services/post.service';
import { Post } from '../../../shared/models/models';

@Component({
  selector: 'app-my-posts',
  templateUrl: './my-posts.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class MyPostsComponent implements OnInit {
  posts: Post[] = [];
  isLoading = false;

  constructor(
    private postService: PostService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadPosts();
  }

  loadPosts(): void {
    this.isLoading = true;
    this.postService.getMyPosts().subscribe({
      next: (response) => {
        this.posts = response.data ?? [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading my posts:', err);
        this.isLoading = false;
      }
    });
  }

  viewPost(postId: string): void {
    this.router.navigate(['/posts', postId]);
  }

  editPost(postId: string, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/posts', postId, 'edit']);
  }

  deletePost(post: Post, event: Event): void {
    event.stopPropagation();
    if (!confirm(`Delete "${post.title}"?`)) return;

    this.postService.deletePost(post.id).subscribe({
      next: () => {
        this.posts = this.posts.filter(p => p.id !== post.id);
      },
      error: (err) => {
        console.error('Error deleting post:', err);
        alert('Failed to delete post');
      }
    });
  }

  createPost(): void {
    this.router.navigate(['/posts/create']);
  }
}
