import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { CommentService } from './comment.service';
import { environment } from '../../../../environments/environment';

describe('CommentService', () => {
  let service: CommentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(CommentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getByPost() requests comments for the given post', () => {
    service.getByPost('post-1').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/posts/post-1/comments`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, statusCode: 200, message: '', data: [], errors: [] });
  });

  it('create() posts the comment content to the post', () => {
    service.create('post-1', { content: 'Nice!' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/posts/post-1/comments`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ content: 'Nice!' });
    req.flush({ success: true, statusCode: 201, message: '', data: null, errors: [] });
  });

  it('delete() sends a DELETE to the comment URL', () => {
    service.delete('comment-1').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/comments/comment-1`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true, statusCode: 200, message: '', data: null, errors: [] });
  });
});
