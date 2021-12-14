import { TestBed } from '@angular/core/testing';
import { slikaService } from './slika service';

describe('slikaService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: slikaService = TestBed.get(slikaService);
    expect(service).toBeTruthy();
  });
});