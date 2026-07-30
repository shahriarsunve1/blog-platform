import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TaxonomyService } from './taxonomy.service';
import { environment } from '../../../../environments/environment';

describe('TaxonomyService', () => {
  let service: TaxonomyService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(TaxonomyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getCategories() fetches the category list', () => {
    service.getCategories().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categories`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, statusCode: 200, message: '', data: [], errors: [] });
  });

  it('createCategory() posts the new category name', () => {
    service.createCategory({ name: 'Science' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categories`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'Science' });
    req.flush({ success: true, statusCode: 201, message: '', data: null, errors: [] });
  });

  it('createTag() posts the new tag name', () => {
    service.createTag({ name: 'Deep Dive' }).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/tags`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ name: 'Deep Dive' });
    req.flush({ success: true, statusCode: 201, message: '', data: null, errors: [] });
  });
});
