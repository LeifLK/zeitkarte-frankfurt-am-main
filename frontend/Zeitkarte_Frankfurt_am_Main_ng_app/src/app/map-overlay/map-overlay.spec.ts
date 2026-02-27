import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MapOverlay } from './map-overlay';

describe('MapOverlay', () => {
  let component: MapOverlay;
  let fixture: ComponentFixture<MapOverlay>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MapOverlay]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MapOverlay);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
