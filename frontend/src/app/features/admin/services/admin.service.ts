import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AdminDashboard, ApiResponse } from '../../../shared/models/models';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private readonly apiUrl = `${environment.apiUrl}/admin`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<ApiResponse<AdminDashboard>> {
    return this.http.get<ApiResponse<AdminDashboard>>(`${this.apiUrl}/dashboard`);
  }
}
