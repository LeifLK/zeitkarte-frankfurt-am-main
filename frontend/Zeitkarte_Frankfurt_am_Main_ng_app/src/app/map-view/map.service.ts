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

    // private geoJson$ = this.http.get<IsochroneGeoJson>('http://localhost:5003/api/Isochrones');
    // geoJson = toSignal(this.geoJson$);

    // private blobJson$ = this.http.get<string>('http://localhost:5003/api/Isochrones/Blob');
    // blobJson = toSignal(this.blobJson$);

    // private Isochrones$ = this.http.get<string>('http://localhost:5003/api/Isochrones/F%20Bockenheimer%20Warte/2000');
    // Isochrones = toSignal(this.Isochrones$);

    getIsochrones(stationId: string, duration: number): Observable<IsochroneGeoJson> {
        const encodedStation = encodeURIComponent(stationId);
    
        const url = `http://localhost:5003/api/Isochrones/${encodedStation}/${duration}`;
        console.log(url);
    
        return this.http.get<IsochroneGeoJson>(url);
    }
}