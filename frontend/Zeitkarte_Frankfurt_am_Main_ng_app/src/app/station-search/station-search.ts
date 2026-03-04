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

  activeIndex = signal(-1);

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
    this.activeIndex.set(-1);
    this.showDropdown.set(true);
  }

  onBlur() {
  setTimeout(() => {
      this.showDropdown.set(false);
    }, 200);
  }

onKeyDown(event: KeyboardEvent) {
  const results = this.searchResults();
  
    if (!this.showDropdown() || results.length === 0) {
      if (event.key === 'Enter') event.preventDefault();
      return;
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      this.activeIndex.update(i => Math.min(i + 1, results.length - 1));
    } 
    else if (event.key === 'ArrowUp') {
      event.preventDefault();
      this.activeIndex.update(i => Math.max(i - 1, 0));
    } 
    else if (event.key === 'Enter') {
      event.preventDefault();
      const currentIdx = this.activeIndex();
      if (currentIdx >= 0) {
        this.selectStation(results[currentIdx]);
      }
    }
  }
}
