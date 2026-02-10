import { Component, effect, inject } from '@angular/core';
import { MapComponent, GeoJSONSourceComponent, LayerComponent } from '@maplibre/ngx-maplibre-gl';
import { MapService } from './map.service';

@Component({
    selector: 'app-map-view',
    imports: [MapComponent, GeoJSONSourceComponent, LayerComponent],
    templateUrl: './map-view.html',
    styleUrl: './map-view.scss',
})
export class MapView {
    mapService = inject(MapService);
    geometry = this.mapService.geoJson;
    multiPolygon = this.mapService.blobJson;


    constructor() {
        effect(() => {
            console.log("Data arrived:", this.geometry());
            console.log("multiPolygon", this.multiPolygon());
        });
    }
}
