import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { IsochroneGeoJson } from '../models/geo-data.model';
import { toSignal } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})

export class MapService {
    private http = inject(HttpClient);

    getIsochrones(stationId: number, duration: number): Observable<IsochroneGeoJson> {
    
        const url = `http://localhost:5003/api/Isochrones/${stationId}/${duration}`;
    
        return this.http.get<IsochroneGeoJson>(url);
    }
}