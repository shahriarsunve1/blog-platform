import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ApiResponse, MediaFileDto } from '../../../shared/models/models';

/**
 * Uploads images so they can be embedded in post content
 */
@Injectable({
  providedIn: 'root'
})
export class MediaService {
  private readonly apiUrl = `${environment.apiUrl}/media`;

  constructor(private http: HttpClient) { }

  upload(file: File): Observable<ApiResponse<MediaFileDto>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ApiResponse<MediaFileDto>>(this.apiUrl, formData);
  }
}
