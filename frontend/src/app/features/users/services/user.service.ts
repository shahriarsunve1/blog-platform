import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  ApiResponse,
  ChangePasswordRequest,
  UpdatePreferencesRequest,
  UpdateProfileRequest,
  User
} from '../../../shared/models/models';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getById(id: string): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.apiUrl}/${id}`);
  }

  follow(userId: string): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/${userId}/follow`, {});
  }

  unfollow(userId: string): Observable<ApiResponse<number>> {
    return this.http.delete<ApiResponse<number>>(`${this.apiUrl}/${userId}/follow`);
  }

  updateProfile(request: UpdateProfileRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/me`, request);
  }

  changePassword(request: ChangePasswordRequest): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.apiUrl}/me/password`, request);
  }

  updatePreferences(request: UpdatePreferencesRequest): Observable<ApiResponse<User>> {
    return this.http.put<ApiResponse<User>>(`${this.apiUrl}/me/preferences`, request);
  }
}
