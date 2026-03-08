import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Station } from '../models/station.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class StationService {
  private http = inject(HttpClient);

  search(term: string) {
    return this.http.get<Station[]>(`${environment.apiUrl}/api/Isochrones/search?q=${term}`);
  }
}
