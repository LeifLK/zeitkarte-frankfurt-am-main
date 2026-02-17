import { Component, output, signal } from '@angular/core';

@Component({
  selector: 'app-duration-slider',
  imports: [],
  templateUrl: './duration-slider.html',
  styleUrl: './duration-slider.scss',
})
export class DurationSlider {
  duration = signal(0);

  durationChanged = output<number>();
  back = output<void>();

  onSlide(event: Event) {
    const val = parseInt((event.target as HTMLInputElement).value, 10);

    this.duration.set(val);
    this.durationChanged.emit(val);
  }

}
