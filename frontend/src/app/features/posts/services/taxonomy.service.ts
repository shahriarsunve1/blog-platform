import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, Category, Tag } from '../../../shared/models/models';

/**
 * Fetches the fixed set of categories/tags posts can be filed under.
 */
@Injectable({
  providedIn: 'root'
})
export class TaxonomyService {
  constructor(private http: HttpClient) {}

  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(`${environment.apiUrl}/categories`);
  }

  getTags(): Observable<ApiResponse<Tag[]>> {
    return this.http.get<ApiResponse<Tag[]>>(`${environment.apiUrl}/tags`);
  }
}
