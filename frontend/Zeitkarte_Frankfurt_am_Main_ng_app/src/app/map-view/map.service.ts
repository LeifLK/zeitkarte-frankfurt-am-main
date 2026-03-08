import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { IsochroneGeoJson } from '../models/geo-data.model';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})

export class MapService {
    private http = inject(HttpClient);

    getIsochrones(stationId: number, duration: number): Observable<IsochroneGeoJson> {
    
        const url = `${environment.apiUrl}/api/Isochrones/${stationId}/${duration}`;
    
        return this.http.get<IsochroneGeoJson>(url);
    }
}