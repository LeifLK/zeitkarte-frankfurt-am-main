BEGIN;

-- 1. Keep only stops within ~20 km of Frankfurt (buffer beyond the app's 15 km)
DELETE FROM stops
WHERE NOT ST_DWithin(
    geom::geography,
    ST_SetSRID(ST_MakePoint(8.682, 50.110), 4326)::geography,
    20000
);

-- 2. Drop stop_times for stops we removed
DELETE FROM stop_times
WHERE stop_id NOT IN (SELECT stop_id FROM stops);

-- 3. Drop orphaned trips
DELETE FROM trips
WHERE trip_id NOT IN (SELECT DISTINCT trip_id FROM stop_times);

-- 4. Drop orphaned routes
DELETE FROM routes
WHERE route_id NOT IN (SELECT DISTINCT route_id FROM trips);

-- 5. Drop unused service calendars
DELETE FROM calendar
WHERE service_id NOT IN (SELECT DISTINCT service_id FROM trips);
DELETE FROM calendar_dates
WHERE service_id NOT IN (SELECT DISTINCT service_id FROM trips);

COMMIT;

VACUUM FULL;
ANALYZE;