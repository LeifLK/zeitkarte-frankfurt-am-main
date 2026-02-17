import { Component, effect, inject, signal } from '@angular/core';
import { MapComponent, GeoJSONSourceComponent, LayerComponent } from '@maplibre/ngx-maplibre-gl';
import { IsochroneGeoJson } from '../models/geo-data.model';

@Component({
    selector: 'app-map-view',
    imports: [MapComponent, GeoJSONSourceComponent, LayerComponent],
    templateUrl: './map-view.html',
    styleUrl: './map-view.scss',
})
export class MapView {
    currentIsochrone = signal<IsochroneGeoJson | null>(null);

    updateMap(data: IsochroneGeoJson | null) {
    this.currentIsochrone.set(data);
  }

    // mapService = inject(MapService);
    // // geometry = this.mapService.geoJson;
    // // isochrones = this.mapService.Isochrones;


    // constructor() {
    //     effect(() => {
    //         // console.log("Data arrived:", this.geometry());
    //         console.log("multiPolygon", this.isochrones());
    //     });
    // }
}
