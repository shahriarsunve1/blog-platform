import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Post } from '../../../shared/models/models';

const MAX_VISIBLE_TAGS = 3;

@Component({
  selector: 'app-post-card',
  templateUrl: './post-card.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [CommonModule, RouterModule]
})
export class PostCardComponent {
  @Input() post!: Post;
  @Output() click = new EventEmitter<void>();

  onClick(): void {
    this.click.emit();
  }

  get visibleTags(): { label: string; isCategory: boolean }[] {
    const combined = [
      ...this.post.categories.map(label => ({ label, isCategory: true })),
      ...this.post.tags.map(label => ({ label, isCategory: false }))
    ];
    return combined.slice(0, MAX_VISIBLE_TAGS);
  }

  get hiddenTagCount(): number {
    const total = this.post.categories.length + this.post.tags.length;
    return Math.max(0, total - MAX_VISIBLE_TAGS);
  }
}
