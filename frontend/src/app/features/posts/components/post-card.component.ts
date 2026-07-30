import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Post } from '../../../shared/models/models';

@Component({
  selector: 'app-post-card',
  templateUrl: './post-card.component.html',
  styleUrls: ['./posts.scss'],
  standalone: true,
  imports: [CommonModule]
})
export class PostCardComponent {
  @Input() post!: Post;
  @Output() click = new EventEmitter<void>();

  onClick(): void {
    this.click.emit();
  }
}
