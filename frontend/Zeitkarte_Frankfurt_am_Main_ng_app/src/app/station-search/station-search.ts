import { Component, effect, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StationService } from './station.service';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { debounceTime, distinct, distinctUntilChanged, of, switchMap } from 'rxjs';
import { Station } from '../models/station.model';

@Component({
  selector: 'app-station-search',
  imports: [FormsModule],
  templateUrl: './station-search.html',
  styleUrl: './station-search.scss',
})
export class StationSearch {
  stationService = inject(StationService)
  searchTerm = signal('');
  showDropdown = signal(false);
  selectedStation = output<Station>();

  searchResults = toSignal(
    toObservable(this.searchTerm).pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (term.length < 2) return of([]);
        return this.stationService.search(term);
      })
    ),
    { initialValue: [] }
  );

  selectStation(station: Station) {
    this.searchTerm.set(station.displayName);
    this.showDropdown.set(false);

    this.selectedStation.emit(station);

    console.log('Selected:', station.displayName);
    }
  
  onType(e: Event) { 
    this.searchTerm.set((e.target as HTMLInputElement).value); 
  }

  onBlur() {
  setTimeout(() => {
    this.showDropdown.set(false);
  }, 200);
}

  // constructor() {
  //       effect(() => {
  //           // console.log("Data arrived:", this.geometry());
  //           console.log("searchTerm", this.searchTerm());
  //       });
  //   }
}
