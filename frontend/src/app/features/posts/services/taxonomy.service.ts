import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, Category, CreateCategoryDto, CreateTagDto, Tag } from '../../../shared/models/models';

/**
 * Fetches (and lets logged-in users add to) the set of categories/tags posts
 * can be filed under.
 */
@Injectable({
  providedIn: 'root'
})
export class TaxonomyService {
  constructor(private http: HttpClient) {}

  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(`${environment.apiUrl}/categories`);
  }

  createCategory(request: CreateCategoryDto): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(`${environment.apiUrl}/categories`, request);
  }

  getTags(): Observable<ApiResponse<Tag[]>> {
    return this.http.get<ApiResponse<Tag[]>>(`${environment.apiUrl}/tags`);
  }

  createTag(request: CreateTagDto): Observable<ApiResponse<Tag>> {
    return this.http.post<ApiResponse<Tag>>(`${environment.apiUrl}/tags`, request);
  }
}
