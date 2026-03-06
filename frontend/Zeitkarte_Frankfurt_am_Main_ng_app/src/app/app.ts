import { Component, signal } from '@angular/core';
import { MapView } from './map-view/map-view';
import { MapOverlay } from "./map-overlay/map-overlay";
import { Header } from "./header/header";
import { Info } from "./info/info";

@Component({
  selector: 'app-root',
  imports: [MapView, MapOverlay, MapOverlay, Header, Info],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('Zeitkarte_Frankfurt_am_Main_ng_app');
  isInfoOpen = false;
}
