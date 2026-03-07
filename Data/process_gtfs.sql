ALTER TABLE stop_times 
ALTER COLUMN stop_sequence TYPE INTEGER 
USING stop_sequence::integer;

ANALYZE stop_times;

CREATE INDEX IF NOT EXISTS idx_stop_times_trip_id ON stop_times(trip_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_stop_id ON stop_times(stop_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_seq ON stop_times(stop_sequence);
CREATE INDEX IF NOT EXISTS idx_stops_stop_id ON stops(stop_id);
CREATE INDEX IF NOT EXISTS idx_trips_route_id ON trips(route_id);

DROP TABLE IF EXISTS derived_shapes;

CREATE UNLOGGED TABLE derived_shapes AS
SELECT DISTINCT
    s1.stop_id AS from_stop_id,
    s2.stop_id AS to_stop_id,
    ST_MakeLine(s1.geom, s2.geom) AS geom
FROM 
    stop_times st1
JOIN 
    stop_times st2 ON st1.trip_id = st2.trip_id 
                   AND st2.stop_sequence = st1.stop_sequence + 1
JOIN 
    stops s1 ON st1.stop_id = s1.stop_id
JOIN 
    stops s2 ON st2.stop_id = s2.stop_id;

CREATE INDEX derived_shapes_geom_idx ON derived_shapes USING GIST (geom);

ANALYZE;