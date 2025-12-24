#!/usr/bin/env python3
"""
This script loads a GTFS zip file into a PostgreSQL/PostGIS database.

It uses pandas.to_sql for fast bulk-loading and then runs
SQL commands to add PostGIS geometry columns and, most importantly,
critical database indexes for routing performance.

VERSION 2: Now robustly handles missing optional files like 'shapes.txt'.
"""

import zipfile
import io
import pandas as pd
from sqlalchemy import create_engine, text
import time

# --- CONFIGURATION ---
GTFS_ZIP_PATH = "/Users/leif/Documents/Dev/RMZeitkarte/Data/gfts_nahverkehr.zip"
DATABASE_URL = "postgresql://localhost/rmzeitkarte"

# --- CONNECT TO POSTGRESQL ---
try:
    engine = create_engine(DATABASE_URL)
    with engine.connect() as conn:
        print("Successfully connected to PostgreSQL database.")
except Exception as e:
    print(f"Error: Could not connect to database at {DATABASE_URL}")
    print(f"Details: {e}")
    print("Please ensure PostgreSQL is running and the database 'rmzeitkarte' exists.")
    exit(1)

# --- HELPER FUNCTIONS ---
def load_csv_to_sql(table_name, csv_bytes):
    """Loads a CSV bytestream into a SQL table using pandas."""
    try:
        df = pd.read_csv(io.BytesIO(csv_bytes))
        # This 'replace' is key. It automatically drops the old table
        # if it exists, ensuring a clean import every time.
        df.to_sql(table_name, engine, if_exists='replace', index=False)
        print(f"Loaded {table_name} ({len(df)} rows)")
    except pd.errors.EmptyDataError:
        print(f"Warning: {table_name}.txt is empty. Skipping.")
    except Exception as e:
        print(f"Error loading {table_name}: {e}")

def add_postgis_and_indexes():
    """
    Adds PostGIS geometry columns and critical indexes for performance.
    This is the most important step for a fast routing engine.
    """
    print("\nStarting PostGIS and Indexing step...")
    try:
        with engine.connect() as conn:
            # Get a list of all tables that were *actually* loaded
            table_names = engine.dialect.get_table_names(conn)
            
            # Use a transaction so all commands succeed or fail together
            with conn.begin():
                print("1/4: Adding geometry column to 'stops'...")
                conn.execute(text("""
                    ALTER TABLE stops
                    ADD COLUMN IF NOT EXISTS geom geometry(Point, 4326);
                    
                    UPDATE stops
                    SET geom = ST_SetSRID(ST_MakePoint(stop_lon, stop_lat), 4326)
                    WHERE geom IS NULL;
                """))

                # --- NEW: Conditional Shapes Block ---
                if 'shapes' in table_names:
                    print("2/4: Adding geometry column to 'shapes' (this may take a moment)...")
                    conn.execute(text("""
                        ALTER TABLE shapes
                        ADD COLUMN IF NOT EXISTS geom geometry(LineString, 4326);
                    """))
                    
                    conn.execute(text("""
                        UPDATE shapes
                        SET geom = s.geom
                        FROM (
                            SELECT
                                shape_id,
                                ST_MakeLine(ST_SetSRID(ST_MakePoint(shape_pt_lon, shape_pt_lat), 4326) ORDER BY shape_pt_sequence) AS geom
                            FROM shapes
                            GROUP BY shape_id
                        ) AS s
                        WHERE shapes.shape_id = s.shape_id AND shapes.geom IS NULL;
                    """))
                else:
                    print("2/4: 'shapes' table not found in GTFS. Skipping shapes processing.")
                
                print("3/4: Creating critical database indexes (this is the most important part!)...")
                
                # Indexes for 'stops'
                conn.execute(text("CREATE INDEX IF NOT EXISTS stops_stop_id_idx ON stops (stop_id);"))
                conn.execute(text("CREATE INDEX IF NOT EXISTS stops_geom_idx ON stops USING GIST (geom);"))
                
                # Indexes for 'routes' and 'trips'
                conn.execute(text("CREATE INDEX IF NOT EXISTS routes_route_id_idx ON routes (route_id);"))
                conn.execute(text("CREATE INDEX IF NOT EXISTS trips_trip_id_idx ON trips (trip_id);"))
                conn.execute(text("CREATE INDEX IF NOT EXISTS trips_route_id_idx ON trips (route_id);"))
                
                # NEW: Only index trips.shape_id if shapes exists
                if 'shapes' in table_names:
                    conn.execute(text("CREATE INDEX IF NOT EXISTS trips_shape_id_idx ON trips (shape_id);"))
                
                # Index for 'calendar'
                if 'calendar' in table_names:
                    conn.execute(text("CREATE INDEX IF NOT EXISTS calendar_service_id_idx ON calendar (service_id);"))
                
                # Index for 'calendar_dates'
                if 'calendar_dates' in table_names:
                    conn.execute(text("CREATE INDEX IF NOT EXISTS calendar_dates_service_id_idx ON calendar_dates (service_id);"))

                # *** THE MOST IMPORTANT INDEXES ***
                # For 'stop_times', the largest table
                print("4/4: Indexing 'stop_times' (this will take the most time)...")
                conn.execute(text("CREATE INDEX IF NOT EXISTS stop_times_stop_id_idx ON stop_times (stop_id);"))
                conn.execute(text("CREATE INDEX IF NOT EXISTS stop_times_trip_id_idx ON stop_times (trip_id);"))
                conn.execute(text("CREATE INDEX IF NOT EXISTS stop_times_departure_time_idx ON stop_times (departure_time);"))

        print("\n\033[92mAll PostGIS and Indexing commands completed successfully!\033[0m")
    
    except Exception as e:
        print(f"\n\033[91mError during PostGIS/Indexing step: {e}\033[0m")
        print("The database may be in an partially indexed state.")

# --- MAIN SCRIPT ---
def main():
    start_time = time.time()
    print(f"Opening GTFS zip file at {GTFS_ZIP_PATH}...")
    
    try:
        with zipfile.ZipFile(GTFS_ZIP_PATH, 'r') as z:
            file_list = [name for name in z.namelist() if name.endswith('.txt')]
            print(f"Found {len(file_list)} .txt files. Starting table import...")
            
            for file_name in file_list:
                with z.open(file_name) as f:
                    table_name = file_name.replace('.txt', '').split('/')[-1]
                    load_csv_to_sql(table_name, f.read())
        
        print("\nAll tables loaded successfully.")
        
        add_postgis_and_indexes()
        
        end_time = time.time()
        print(f"\nTotal script finished in {end_time - start_time:.2f} seconds.")

    except zipfile.BadZipFile:
        print(f"Error: Could not read zip file at {GTFS_ZIP_PATH}.")
    except FileNotFoundError:
        print(f"Error: File not found at {GTFS_ZIP_PATH}.")
    except Exception as e:
        print(f"An unexpected error occurred: {e}")

if __name__ == "__main__":
    main()

