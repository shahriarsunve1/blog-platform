import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PostService } from '../services/post.service';
import { Post } from '../../../shared/models/models';
import { PostCardComponent } from './post-card.component';

@Component({
  selector: 'app-suggested-for-you',
  templateUrl: './suggested-for-you.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [CommonModule, PostCardComponent]
})
export class SuggestedForYouComponent implements OnInit {
  posts: Post[] = [];
  isLoading = true;

  constructor(private postService: PostService, private router: Router) {}

  ngOnInit(): void {
    this.postService.getSuggested(6).subscribe({
      next: (response) => {
        this.posts = response.data ?? [];
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading suggested posts:', err);
        this.isLoading = false;
      }
    });
  }

  viewPost(postId: string): void {
    this.router.navigate(['/posts', postId]);
  }
}
