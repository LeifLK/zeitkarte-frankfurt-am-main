import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { IsochroneGeoJson } from '../models/geo-data.model';
import { toSignal } from '@angular/core/rxjs-interop';

@Injectable({
    providedIn: 'root'
})

export class MapService {
    private http = inject(HttpClient);

    // private geoJson$ = this.http.get<IsochroneGeoJson>('http://localhost:5003/api/Isochrones');
    // geoJson = toSignal(this.geoJson$);

    // private blobJson$ = this.http.get<string>('http://localhost:5003/api/Isochrones/Blob');
    // blobJson = toSignal(this.blobJson$);

    private Isochrones$ = this.http.get<string>('http://localhost:5003/api/Isochrones/F%20Bockenheimer%20Warte/2500');
    Isochrones = toSignal(this.Isochrones$);
}