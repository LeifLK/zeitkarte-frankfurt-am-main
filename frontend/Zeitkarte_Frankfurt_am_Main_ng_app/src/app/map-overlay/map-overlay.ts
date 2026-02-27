import { Component, inject, output, signal } from '@angular/core';
import { Station } from '../models/station.model';
import { StationSearch } from "../station-search/station-search";
import { DurationSlider } from '../duration-slider/duration-slider';
import { MapService } from '../map-view/map.service';
import { IsochroneGeoJson } from '../models/geo-data.model';

@Component({
  selector: 'app-map-overlay',
  imports: [StationSearch, DurationSlider],
  templateUrl: './map-overlay.html',
  styleUrl: './map-overlay.scss',
})
export class MapOverlay {
  mapService = inject(MapService);
  
  selectedStation = signal<Station | null>(null);
  selectedDuration = signal<number>(15);
  mapDataLoaded = output<IsochroneGeoJson | null>();

  onStationSelect(station: Station) {
    this.selectedStation.set(station);

    const duration = this.selectedDuration(); 
    this.onDurationChange(duration);
  }

  onBack() {
    this.selectedStation.set(null);
    this.selectedDuration.set(15);
    this.mapDataLoaded.emit(null);
  }

  onDurationChange(duration: number) {
    const durationSeconds = duration * 60;
    const currentStation = this.selectedStation();
  

    if (currentStation){
      this.mapService.getIsochrones(currentStation.id, durationSeconds)
        .subscribe(geoJson => {
          this.mapDataLoaded.emit(geoJson);
        });

    }
  }
}
