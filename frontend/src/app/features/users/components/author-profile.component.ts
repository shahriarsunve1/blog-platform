import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';
import { PostService } from '../../posts/services/post.service';
import { AuthService } from '../../../core/auth/auth.service';
import { Post, User } from '../../../shared/models/models';
import { PostCardComponent } from '../../posts/components/post-card.component';

@Component({
  selector: 'app-author-profile',
  templateUrl: './author-profile.component.html',
  styleUrls: ['./author-profile.component.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule, PostCardComponent]
})
export class AuthorProfileComponent implements OnInit {
  author: User | null = null;
  posts: Post[] = [];
  isLoading = true;
  postsLoading = false;
  isTogglingFollow = false;
  currentUserId: string | null = null;

  constructor(
    private userService: UserService,
    private postService: PostService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUser()?.id ?? null;

    const authorId = this.route.snapshot.paramMap.get('id')!;
    this.loadAuthor(authorId);
    this.loadPosts(authorId);
  }

  get isOwnProfile(): boolean {
    return !!this.currentUserId && this.currentUserId === this.author?.id;
  }

  loadAuthor(authorId: string): void {
    this.isLoading = true;
    this.userService.getById(authorId).subscribe({
      next: (response) => {
        this.author = response.data ?? null;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading author:', err);
        this.isLoading = false;
        this.router.navigate(['/posts']);
      }
    });
  }

  loadPosts(authorId: string): void {
    this.postsLoading = true;
    this.postService.getPublishedPosts(1, 20, undefined, undefined, undefined, authorId).subscribe({
      next: (response) => {
        this.posts = response.data?.items ?? [];
        this.postsLoading = false;
      },
      error: (err) => {
        console.error('Error loading author posts:', err);
        this.postsLoading = false;
      }
    });
  }

  toggleFollow(): void {
    if (!this.author || !this.currentUserId) return;

    const wasFollowed = this.author.isFollowedByCurrentUser;
    this.isTogglingFollow = true;
    // Optimistic update
    this.author.isFollowedByCurrentUser = !wasFollowed;
    this.author.followerCount = (this.author.followerCount ?? 0) + (wasFollowed ? -1 : 1);

    const request$ = wasFollowed ? this.userService.unfollow(this.author.id) : this.userService.follow(this.author.id);
    request$.subscribe({
      next: (response) => {
        if (this.author && response.data !== undefined) {
          this.author.followerCount = response.data;
        }
        this.isTogglingFollow = false;
      },
      error: (err) => {
        console.error('Error toggling follow:', err);
        if (this.author) {
          this.author.isFollowedByCurrentUser = wasFollowed;
          this.author.followerCount = (this.author.followerCount ?? 0) + (wasFollowed ? 1 : -1);
        }
        this.isTogglingFollow = false;
      }
    });
  }

  viewPost(postId: string): void {
    this.router.navigate(['/posts', postId]);
  }
}
