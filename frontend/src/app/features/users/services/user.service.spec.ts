import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { UserService } from './user.service';
import { environment } from '../../../../environments/environment';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/users`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getById() requests the user profile', () => {
    service.getById('user-1').subscribe();
    const req = httpMock.expectOne(`${apiUrl}/user-1`);
    expect(req.request.method).toBe('GET');
    req.flush({ success: true, statusCode: 200, message: '', data: null, errors: [] });
  });

  it('follow() posts to the follow endpoint', () => {
    service.follow('user-1').subscribe();
    const req = httpMock.expectOne(`${apiUrl}/user-1/follow`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true, statusCode: 200, message: '', data: 1, errors: [] });
  });

  it('unfollow() deletes the follow endpoint', () => {
    service.unfollow('user-1').subscribe();
    const req = httpMock.expectOne(`${apiUrl}/user-1/follow`);
    expect(req.request.method).toBe('DELETE');
    req.flush({ success: true, statusCode: 200, message: '', data: 0, errors: [] });
  });
});
