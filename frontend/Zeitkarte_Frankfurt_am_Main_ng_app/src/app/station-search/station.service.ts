import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Station } from '../models/station.model';

@Injectable({
  providedIn: 'root',
})
export class StationService {
  private http = inject(HttpClient);

  search(term: string) {
    return this.http.get<Station[]>(`http://localhost:5003/api/Isochrones/search?q=${term}`);
  }
}
