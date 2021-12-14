import { TestBed } from '@angular/core/testing';
import { testService } from './test service';

describe('testService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: testService = TestBed.get(testService);
    expect(service).toBeTruthy();
  });
});