import { FeatureCollection, Point } from 'geojson';

export interface IsochroneProperties {
    stopId: string;
    duration: number;
    originId: string;
}

export type IsochroneGeoJson = FeatureCollection<Point, IsochroneProperties>;