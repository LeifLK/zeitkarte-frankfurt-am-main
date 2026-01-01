import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MapView } from './map-view/map-view';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MapView],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Zeitkarte_Frankfurt_am_Main_ng_app');
}
