import { TestBed } from '@angular/core/testing';
import { bijemService } from './bijem service';

describe('bijemService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: bijemService = TestBed.get(bijemService);
    expect(service).toBeTruthy();
  });
});