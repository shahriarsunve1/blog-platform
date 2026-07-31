/**
 * User authentication models
 */
export interface User {
  id: string;
  username: string;
  email: string;
  firstName: string;
  lastName: string;
  bio: string;
  avatar: string;
  role: string;
  createdAt: string;
  followerCount?: number;
  isFollowedByCurrentUser?: boolean;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}

/**
 * Blog post models
 */
export interface Post {
  id: string;
  userId: string;
  title: string;
  excerpt: string;
  content: string;
  status: PostStatus;
  viewCount: number;
  author?: User;
  categories: string[];
  tags: string[];
  likeCount: number;
  isLikedByCurrentUser: boolean;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string;
}

export interface CreatePostDto {
  title: string;
  excerpt: string;
  content: string;
  status: PostStatus;
  categoryIds: string[];
  tagIds: string[];
}

export interface UpdatePostDto extends CreatePostDto {}

export interface Category {
  id: string;
  name: string;
  slug: string;
}

export interface CreateCategoryDto {
  name: string;
}

export interface Tag {
  id: string;
  name: string;
  slug: string;
}

export interface CreateTagDto {
  name: string;
}

export enum PostStatus {
  Draft = 'Draft',
  Published = 'Published',
  Archived = 'Archived'
}

/**
 * Comment models
 */
export interface Comment {
  id: string;
  postId: string;
  content: string;
  author?: User;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCommentDto {
  content: string;
}

/**
 * API response models
 */
export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data?: T;
  errors?: ErrorDetail[];
}

export interface ErrorDetail {
  field: string;
  message: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
