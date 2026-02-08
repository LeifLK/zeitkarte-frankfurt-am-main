import { FeatureCollection, Point } from 'geojson';

export interface IsochroneProperties {
    stopId: string;
    duration: number;
    originId: string;
}

// Type Alias combining the standard structure with my custom data
// FeatureCollection<GeometryType, PropertiesType>
export type IsochroneGeoJson = FeatureCollection<Point, IsochroneProperties>;