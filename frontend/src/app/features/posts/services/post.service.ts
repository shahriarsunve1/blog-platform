import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Post, CreatePostDto, UpdatePostDto, ApiResponse, PaginatedResponse } from '../../../shared/models/models';

/**
 * Post service
 */
@Injectable({
  providedIn: 'root'
})
export class PostService {
  private readonly apiUrl = `${environment.apiUrl}/posts`;

  constructor(private http: HttpClient) { }

  /**
   * Get all published posts
   */
  getPublishedPosts(
    pageNumber: number = 1,
    pageSize: number = 10,
    categoryId?: string,
    tagId?: string,
    search?: string
  ): Observable<ApiResponse<PaginatedResponse<Post>>> {
    let url = `${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`;
    if (categoryId) url += `&categoryId=${categoryId}`;
    if (tagId) url += `&tagId=${tagId}`;
    if (search) url += `&search=${encodeURIComponent(search)}`;
    return this.http.get<ApiResponse<PaginatedResponse<Post>>>(url);
  }

  /**
   * Get single post by ID
   */
  getPostById(id: string): Observable<ApiResponse<Post>> {
    return this.http.get<ApiResponse<Post>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create new post
   */
  createPost(request: CreatePostDto): Observable<ApiResponse<Post>> {
    return this.http.post<ApiResponse<Post>>(this.apiUrl, request);
  }

  /**
   * Update post
   */
  updatePost(id: string, request: UpdatePostDto): Observable<ApiResponse<Post>> {
    return this.http.put<ApiResponse<Post>>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete post
   */
  deletePost(id: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get user's posts
   */
  getUserPosts(userId: string): Observable<ApiResponse<Post[]>> {
    return this.http.get<ApiResponse<Post[]>>(`${this.apiUrl}/user/${userId}`);
  }
}
