#!/bin/bash

# Configuration
DB_NAME="zeitkarte_ffm"
DB_USER=$(whoami)
CONN="PG:dbname=$DB_NAME user=$DB_USER"
DATA_DIR="./GTFS"

echo "------------------------------------------"
echo "Starting GTFS Import to $DB_NAME"
echo "Data Source: $DATA_DIR"
echo "------------------------------------------"

# Check if directory exists
if [ ! -d "$DATA_DIR" ]; then
    echo "Error: Directory '$DATA_DIR' does not exist."
    exit 1
fi

# 1. Import stops.txt
# We prefix the path with 'CSV:' to force ogr2ogr to read it as a CSV
if [ -f "$DATA_DIR/stops.txt" ]; then
    echo "Importing stops.txt..."
    ogr2ogr -f PostgreSQL "$CONN" "CSV:$DATA_DIR/stops.txt" \
        -nln stops \
        -overwrite \
        -oo X_POSSIBLE_NAMES=stop_lon \
        -oo Y_POSSIBLE_NAMES=stop_lat \
        -oo KEEP_GEOM_COLUMNS=NO \
        -lco GEOMETRY_NAME=geom \
        -lco SPATIAL_INDEX=GIST
else
    echo "Warning: stops.txt not found in $DATA_DIR"
fi

# # 2. Import shapes.txt (Import as Points first)
# if [ -f "$DATA_DIR/shapes.txt" ]; then
#     echo "Importing shapes.txt..."
#     ogr2ogr -f PostgreSQL "$CONN" "$DATA_DIR/shapes.txt" \
#         -nln shapes_points \
#         -overwrite \
#         -oo X_POSSIBLE_NAMES=shape_pt_lon \
#         -oo Y_POSSIBLE_NAMES=shape_pt_lat \
#         -oo KEEP_GEOM_COLUMNS=NO \
#         -lco GEOMETRY_NAME=geom \
#         -lco SPATIAL_INDEX=NONE
# else
#     echo "Warning: shapes.txt not found in $DATA_DIR"
# fi

# 3. Import Standard Tables (No Geometry)
FILES=("agency" "attributions" "calendar_dates" "calendar" "feed_info" "routes" "trips" "stop_times")

for table in "${FILES[@]}"; do
    file="$DATA_DIR/${table}.txt"
    if [ -f "$file" ]; then
        echo "Importing $file..."
        ogr2ogr -f PostgreSQL "$CONN" "CSV:$file" \
            -nln "$table" \
            -overwrite \
            -nlt NONE
    else
        echo "Skipping $table (file not found)"
    fi
done

# echo "------------------------------------------"
# echo "Running Post-Processing SQL..."
# echo "------------------------------------------"

# # Note: We assume process_gtfs.sql is in the SAME folder as this script, NOT in 'GTFS'
# psql -d "$DB_NAME" -U "$DB_USER" -f process_gtfs.sql

# echo "Done! Database '$DB_NAME' is ready."