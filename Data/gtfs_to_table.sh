#!/bin/bash

DB_NAME="zeitkarte_ffm"
DB_USER=$(whoami)
CONN="PG:dbname=$DB_NAME user=$DB_USER"
DATA_DIR="./latest"

echo "------------------------------------------"
echo "Starting GTFS Import to $DB_NAME"
echo "Data Source: $DATA_DIR"
echo "------------------------------------------"

if [ ! -d "$DATA_DIR" ]; then
    echo "Error: Directory '$DATA_DIR' does not exist."
    exit 1
fi

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
