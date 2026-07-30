import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PostService } from '../services/post.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Post } from '../../../shared/models/models';

@Component({
  selector: 'app-post-detail',
  templateUrl: './post-detail.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class PostDetailComponent implements OnInit {
  post: Post | null = null;
  isLoading = true;
  isAuthor = false;
  currentUserId: string | null = null;

  constructor(
    private postService: PostService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.currentUserId = user.id;
    }

    const postId = this.route.snapshot.paramMap.get('id')!;
    this.loadPost(postId);
  }

  loadPost(postId: string): void {
    this.postService.getPostById(postId).subscribe({
      next: (response) => {
        if (response.data) {
          this.post = response.data;
          this.isAuthor = response.data.userId === this.currentUserId;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading post:', err);
        this.isLoading = false;
        this.router.navigate(['/posts']);
      }
    });
  }

  editPost(): void {
    if (this.post) {
      this.router.navigate(['/posts', this.post.id, 'edit']);
    }
  }

  deletePost(): void {
    if (this.post && confirm('Are you sure you want to delete this post?')) {
      this.postService.deletePost(this.post.id).subscribe({
        next: () => {
          this.router.navigate(['/posts']);
        },
        error: (err) => {
          console.error('Error deleting post:', err);
          alert('Failed to delete post');
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/posts']);
  }
}
