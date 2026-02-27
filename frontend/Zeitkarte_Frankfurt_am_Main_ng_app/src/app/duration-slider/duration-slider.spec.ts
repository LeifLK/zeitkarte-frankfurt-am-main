import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DurationSlider } from './duration-slider';

describe('DurationSlider', () => {
  let component: DurationSlider;
  let fixture: ComponentFixture<DurationSlider>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DurationSlider]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DurationSlider);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
