import { TestBed } from '@angular/core/testing';

import { StationServiceTs } from './station.service.ts';

describe('StationServiceTs', () => {
  let service: StationServiceTs;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StationServiceTs);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
