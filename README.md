# Zeitkarte Frankfurt am Main

A high-performance public transport routing engine for Frankfurt am Main. Select a station and a travel time, and the map highlights the area reachable by U-Bahn, Bus, and Tram, including up to 15 minutes of walking.

## How it works

GTFS schedule data is loaded into a PostGIS database. The backend uses the Connection Scan Algorithm (CSA) to compute travel-time isochrones, which are served via a .NET API. The frontend is built with Angular and renders the map using ngx-maplibre-gl and OpenFreeMap. Transit data is sourced from [gtfs.de](https://gtfs.de) (CC BY 4.0).

## Contributing

Pull requests are welcome!

<img width="909" height="784" alt="image" src="https://github.com/user-attachments/assets/e8f26a97-3f32-40fd-8a63-50d9fda71667" />
