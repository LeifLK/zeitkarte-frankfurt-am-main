import csv
import json
import os

INPUT_CSV_FILE = 'RMV_Haltestellen_Tarifperiode_2025_Stand_2025-04-15.csv'
OUTPUT_JSON_FILE = 'frankfurt_stops.json'
FILTER_TERM = 'Frankfurt (Main)'

def filter_stops():
    # Absolut part for this script
    script_dir = os.path.dirname(os.path.abspath(__file__))
    
    # Full paths for the input and output files
    input_path = os.path.join(script_dir, INPUT_CSV_FILE)
    output_path = os.path.join(script_dir, OUTPUT_JSON_FILE)

    if not os.path.exists(input_path):
        print(f"Error: Input file not found at {input_path}")
        print("Please make sure the CSV file is in the same 'data' folder as this script.")
        return

    frankfurt_stops = []
    total_stops = 0

    print(f"Starting filter process for '{FILTER_TERM}'...")

    # 'utf-8-sig' to handle the BOM (Byte Order Mark)
    with open(input_path, mode='r', encoding='utf-8-sig', newline='') as infile:
        reader = csv.DictReader(infile, delimiter=';')
        
        for row in reader:
            total_stops += 1
            stop_name = row.get('NAME_FAHRPLAN')
            
            if stop_name and stop_name.startswith(FILTER_TERM):
                try:
                    clean_stop = {
                        'Id': row.get('HAFAS_ID'),
                        'Name': stop_name,
                        'Longitude': float(row.get('X_WGS84', '0').replace(',', '.')),
                        'Latitude': float(row.get('Y_WGS84', '0').replace(',', '.'))
                    }
                    frankfurt_stops.append(clean_stop)
                except (ValueError, TypeError) as e:
                    print(f"Warning: Skipping stop '{stop_name}' due to bad coordinate data: {e}")

    print(f"Read {total_stops} total stops from CSV.")
    print(f"\033[92mFound {len(frankfurt_stops)} stops in '{FILTER_TERM}'.\033[0m")

    with open(output_path, 'w', encoding='utf-8') as outfile:
        json.dump(frankfurt_stops, outfile, indent=2, ensure_ascii=False)

    print(f"Successfully saved clean data to '{OUTPUT_JSON_FILE}'.")

if __name__ == "__main__":
    filter_stops()
