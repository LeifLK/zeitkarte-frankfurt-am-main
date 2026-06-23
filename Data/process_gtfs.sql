-- Cast stop_sequence to integer
ALTER TABLE stop_times 
ALTER COLUMN stop_sequence TYPE INTEGER 
USING stop_sequence::integer;

ANALYZE stop_times;

-- stop_times indexes
CREATE INDEX IF NOT EXISTS idx_stop_times_trip_id ON stop_times(trip_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_stop_id ON stop_times(stop_id);
CREATE INDEX IF NOT EXISTS idx_stop_times_seq ON stop_times(stop_sequence);
CREATE INDEX IF NOT EXISTS idx_stop_times_departure_time ON stop_times(departure_time);
CREATE INDEX IF NOT EXISTS idx_stop_times_trip_seq ON stop_times(trip_id, stop_sequence);

-- stops indexes
CREATE INDEX IF NOT EXISTS idx_stops_stop_id ON stops(stop_id);
CREATE INDEX IF NOT EXISTS idx_stops_name ON stops(stop_name);
CREATE INDEX IF NOT EXISTS stops_geom_geom_idx ON stops USING GIST (geom);

-- trips indexes
CREATE INDEX IF NOT EXISTS idx_trips_route_id ON trips(route_id);

-- Required after geographic pruning. The generator joins stop_times -> trips -> calendar.
-- Post-pruning the ST_DWithin filters barely reduce rows, so the planner's low
-- selectivity estimate leads to a nested-loop seq-scan blowup on trips/calendar.
-- These indexes turn each join back into a fast index probe (minutes -> seconds).
CREATE INDEX idx_trips_trip_id ON trips(trip_id);
CREATE INDEX idx_calendar_service_id ON calendar(service_id);

ANALYZE;