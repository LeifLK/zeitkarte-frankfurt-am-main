import { Component } from '@angular/core';
import { MapComponent } from '@maplibre/ngx-maplibre-gl';

@Component({
  selector: 'app-map-view',
  imports: [MapComponent],
  templateUrl: './map-view.html',
  styleUrl: './map-view.scss',
})
export class MapView {

}
