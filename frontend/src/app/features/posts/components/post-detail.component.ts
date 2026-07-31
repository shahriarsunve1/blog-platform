import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostService } from '../services/post.service';
import { CommentService } from '../services/comment.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Comment, Post } from '../../../shared/models/models';

@Component({
  selector: 'app-post-detail',
  templateUrl: './post-detail.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule]
})
export class PostDetailComponent implements OnInit {
  post: Post | null = null;
  isLoading = true;
  isAuthor = false;
  currentUserId: string | null = null;
  isAdmin = false;

  comments: Comment[] = [];
  commentsLoading = false;
  newCommentText = '';
  isPostingComment = false;
  commentError = '';

  linkCopied = false;

  constructor(
    private postService: PostService,
    private commentService: CommentService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      this.currentUserId = user.id;
      this.isAdmin = user.role === 'Admin';
    }

    const postId = this.route.snapshot.paramMap.get('id')!;
    this.loadPost(postId);
    this.loadComments(postId);
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

  loadComments(postId: string): void {
    this.commentsLoading = true;
    this.commentService.getByPost(postId).subscribe({
      next: (response) => {
        this.comments = response.data ?? [];
        this.commentsLoading = false;
      },
      error: (err) => {
        console.error('Error loading comments:', err);
        this.commentsLoading = false;
      }
    });
  }

  canDeleteComment(comment: Comment): boolean {
    return comment.author?.id === this.currentUserId || this.isAdmin;
  }

  toggleLike(): void {
    if (!this.post || !this.currentUserId) return;

    const wasLiked = this.post.isLikedByCurrentUser;
    // Optimistic update - flip immediately, roll back on error.
    this.post.isLikedByCurrentUser = !wasLiked;
    this.post.likeCount += wasLiked ? -1 : 1;

    const request$ = wasLiked ? this.postService.unlike(this.post.id) : this.postService.like(this.post.id);
    request$.subscribe({
      next: (response) => {
        if (this.post && response.data !== undefined) {
          this.post.likeCount = response.data;
        }
      },
      error: (err) => {
        console.error('Error toggling like:', err);
        if (this.post) {
          this.post.isLikedByCurrentUser = wasLiked;
          this.post.likeCount += wasLiked ? 1 : -1;
        }
      }
    });
  }

  get shareUrl(): string {
    return window.location.href;
  }

  shareOnTwitter(): void {
    if (!this.post) return;
    const url = `https://twitter.com/intent/tweet?url=${encodeURIComponent(this.shareUrl)}&text=${encodeURIComponent(this.post.title)}`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }

  shareOnFacebook(): void {
    const url = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(this.shareUrl)}`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }

  shareOnLinkedIn(): void {
    const url = `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(this.shareUrl)}`;
    window.open(url, '_blank', 'noopener,noreferrer');
  }

  copyLink(): void {
    navigator.clipboard.writeText(this.shareUrl).then(() => {
      this.linkCopied = true;
      setTimeout(() => (this.linkCopied = false), 2000);
    });
  }

  postComment(): void {
    if (!this.post || !this.newCommentText.trim()) return;

    this.isPostingComment = true;
    this.commentError = '';

    this.commentService.create(this.post.id, { content: this.newCommentText.trim() }).subscribe({
      next: (response) => {
        if (response.data) {
          this.comments.push(response.data);
        }
        this.newCommentText = '';
        this.isPostingComment = false;
      },
      error: (err) => {
        this.commentError = err.error?.message || 'Failed to post comment';
        this.isPostingComment = false;
      }
    });
  }

  deleteComment(comment: Comment): void {
    if (!confirm('Delete this comment?')) return;

    this.commentService.delete(comment.id).subscribe({
      next: () => {
        this.comments = this.comments.filter(c => c.id !== comment.id);
      },
      error: (err) => {
        console.error('Error deleting comment:', err);
        alert('Failed to delete comment');
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
