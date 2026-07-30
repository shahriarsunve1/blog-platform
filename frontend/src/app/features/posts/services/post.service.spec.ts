import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { PostService } from './post.service';
import { PostStatus } from '../../../shared/models/models';
import { environment } from '../../../../environments/environment';

describe('PostService', () => {
  let service: PostService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/posts`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(PostService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getPublishedPosts() requests the paginated list with query params', () => {
    service.getPublishedPosts(2, 5).subscribe();

    const req = httpMock.expectOne(`${apiUrl}?pageNumber=2&pageSize=5`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, statusCode: 200, message: '', data: { items: [], totalCount: 0, pageNumber: 2, pageSize: 5, totalPages: 0 }, errors: [] });
  });

  it('getPublishedPosts() appends categoryId/tagId when provided', () => {
    service.getPublishedPosts(1, 10, 'cat-1', 'tag-1').subscribe();

    const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=10&categoryId=cat-1&tagId=tag-1`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, statusCode: 200, message: '', data: { items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 }, errors: [] });
  });

  it('getPublishedPosts() appends and URL-encodes the search term', () => {
    service.getPublishedPosts(1, 10, undefined, undefined, 'angular & rxjs').subscribe();

    const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=10&search=${encodeURIComponent('angular & rxjs')}`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, statusCode: 200, message: '', data: { items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 }, errors: [] });
  });

  it('getPostById() requests the specific post', () => {
    service.getPostById('abc-123').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/abc-123`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, statusCode: 200, message: '', data: null, errors: [] });
  });

  it('createPost() posts the payload including category/tag ids', () => {
    const payload = {
      title: 'T', excerpt: 'E', content: 'C', status: PostStatus.Draft,
      categoryIds: ['cat-1'], tagIds: ['tag-1']
    };

    service.createPost(payload).subscribe();

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush({ success: true, statusCode: 201, message: '', data: null, errors: [] });
  });

  it('deletePost() sends a DELETE to the post URL', () => {
    service.deletePost('abc-123').subscribe();

    const req = httpMock.expectOne(`${apiUrl}/abc-123`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true, statusCode: 200, message: '', data: null, errors: [] });
  });
});
