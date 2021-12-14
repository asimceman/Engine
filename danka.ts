import { TestBed } from '@angular/core/testing';
import { dankaService } from './danka service';

describe('dankaService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: dankaService = TestBed.get(dankaService);
    expect(service).toBeTruthy();
  });
});