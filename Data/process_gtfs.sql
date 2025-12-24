ALTER TABLE stop_times 
ALTER COLUMN stop_sequence TYPE INTEGER 
USING stop_sequence::integer;

-- CRITICAL: Update statistics immediately after the column change.
-- Changed from VACUUM ANALYZE to just ANALYZE to prevent transaction errors in GUI tools.
ANALYZE stop_times;

-- 1. Performance Indexing (Do this FIRST)
-- We need these indices immediately to make the geometry generation below fast.
CREATE INDEX IF NOT EXISTS idx_stop_times_trip_id ON stop_times(trip_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_stop_id ON stop_times(stop_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_seq ON stop_times(stop_sequence);
CREATE INDEX IF NOT EXISTS idx_stops_stop_id ON stops(stop_id);
CREATE INDEX IF NOT EXISTS idx_trips_route_id ON trips(route_id);

-- 2. Generate 'derived_shapes' from the Schedule
-- Since we have no shapes.txt, we create lines by connecting 
-- Stop A -> Stop B for every segment in the schedule.

DROP TABLE IF EXISTS derived_shapes;

-- OPTIMIZATION: Use UNLOGGED table for faster writing (skips transaction log)
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

-- Add spatial index to our new derived shapes
CREATE INDEX derived_shapes_geom_idx ON derived_shapes USING GIST (geom);

-- 3. Final Cleanup
-- Changed from VACUUM ANALYZE to just ANALYZE.
ANALYZE;