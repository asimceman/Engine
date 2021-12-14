import { TestBed } from '@angular/core/testing';
import { asimService } from './asim service';

describe('asimService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: asimService = TestBed.get(asimService);
    expect(service).toBeTruthy();
  });
});