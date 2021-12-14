import { TestBed } from '@angular/core/testing';
import { PustiteMeDaZivimService } from './PustiteMeDaZivim service';

describe('PustiteMeDaZivimService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: PustiteMeDaZivimService = TestBed.get(PustiteMeDaZivimService);
    expect(service).toBeTruthy();
  });
});