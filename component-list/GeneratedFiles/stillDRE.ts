import { TestBed } from '@angular/core/testing';
import { stillDREService } from './stillDRE service';

describe('stillDREService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: stillDREService = TestBed.get(stillDREService);
    expect(service).toBeTruthy();
  });
});