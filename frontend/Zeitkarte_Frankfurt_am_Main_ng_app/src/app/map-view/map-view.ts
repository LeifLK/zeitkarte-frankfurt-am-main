import { Component, effect, inject, signal } from '@angular/core';
import { MapComponent, GeoJSONSourceComponent, LayerComponent } from '@maplibre/ngx-maplibre-gl';
import { IsochroneGeoJson } from '../models/geo-data.model';
import { Station } from '../models/station.model';

@Component({
  selector: 'app-map-view',
  imports: [MapComponent, GeoJSONSourceComponent, LayerComponent],
  templateUrl: './map-view.html',
  styleUrl: './map-view.scss',
})
export class MapView {
  currentIsochrone = signal<IsochroneGeoJson | null>(null);
  stationCoordinates = signal<Station | null>(null);

  updateMap(data: IsochroneGeoJson | null) {
    this.currentIsochrone.set(data);
  }

  updateStation(station: Station | null) {
    this.stationCoordinates.set(station);
  }
}
