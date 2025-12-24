using Npgsql;

var rawConnections = new List<(string from, string to, int dep, int arr, int tripId)>();

Dictionary<string, int> StationToId = new Dictionary<string, int>();

List<string> IdToStation = new List<string>();

List<Connection> tempConnList = new List<Connection>();

Dictionary<string, int> TripToId = new Dictionary<string, int>();

string startStation = "F Bockenheimer Warte";

var connectionString = "Host=localhost;Username=leif;Password=;Database=zeitkarte_ffm";
await using var dataSource = NpgsqlDataSource.Create(connectionString);

string sql = @"
    SELECT 
        S1.stop_name as departure_station, 
        S2.stop_name as arrival_station, 
        A.departure_time, 
        B.arrival_time, 
        A.trip_id
    FROM stop_times A
    JOIN stop_times B 
        ON A.trip_id = B.trip_id 
        AND A.stop_sequence + 1 = B.stop_sequence
    JOIN trips T 
        ON A.trip_id = T.trip_id
    JOIN calendar C 
        ON T.service_id = C.service_id
    JOIN stops S1 
        ON A.stop_id = S1.stop_id
    JOIN stops S2 
        ON B.stop_id = S2.stop_id
    WHERE C.monday = '1'
    AND ST_DWithin(
            S1.geom::geography, 
            ST_SetSRID(ST_MakePoint(8.682, 50.110), 4326)::geography, 
            15000
        )
        AND ST_DWithin(
            S2.geom::geography, 
            ST_SetSRID(ST_MakePoint(8.682, 50.110), 4326)::geography, 
            15000
        )
    ORDER BY A.departure_time ASC;
";

await using var command = dataSource.CreateCommand(sql);

command.CommandTimeout = 0;

await using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    string fromStop = reader.GetString(0);
    string toStop = reader.GetString(1);
    string depString = reader.GetString(2);
    string arrString = reader.GetString(3);
    int depSeconds = ParseGtfsTime(depString);
    int arrSeconds = ParseGtfsTime(arrString);
    int tripId = GetTripId(reader.GetString(4));


    Connection conn = new Connection
    {
        DepStationIdx = GetStationId(fromStop),
        ArrStationIdx = GetStationId(toStop),
        DepTime = depSeconds,
        ArrTime = arrSeconds,
        TripId = tripId
    };

    tempConnList.Add(conn);
}


// Sort connections by departure time for csa to work, redundant because db is sorting ASC
tempConnList.Sort((a, b) => a.DepTime.CompareTo(b.DepTime));

var connections = tempConnList.ToArray();

Dictionary<int, int> earliestArrival = csa(connections, startStation, Time(7, 50), TripToId.Count);
PrintArrivalTimes(earliestArrival);

// Imort earliestArrval results in to the database arrival_times
await using var dbConn = await dataSource.OpenConnectionAsync();

// For now just delete everything in db and reload the data to it
await using var truncateCommand = new NpgsqlCommand("TRUNCATE arrival_times", dbConn);
await truncateCommand.ExecuteNonQueryAsync();

await using (var writer = await dbConn.BeginBinaryImportAsync(
    "COPY arrival_times (origin_stop_id, dest_stop_id, duration_seconds) FROM STDIN (FORMAT BINARY)"))
{
    foreach (var kvp in earliestArrival)
    {
        string Arrivalstation = IdToStation[kvp.Key];
        int arrivalTime = kvp.Value;

        int duration = arrivalTime - Time(7, 50);

        await writer.StartRowAsync();
        await writer.WriteAsync(startStation);
        await writer.WriteAsync(Arrivalstation);
        await writer.WriteAsync(duration);
    }
    await writer.CompleteAsync();
    Console.WriteLine("Data saved to Database!");
}









Dictionary<int, int> csa(Connection[] connections, string startStation, int startTime, int maxTrips)
{
    int transferTime = 180;
    Dictionary<int, int> earliestArrival = new Dictionary<int, int>();
    earliestArrival.Add(StationToId["F Bockenheimer Warte"], startTime);

    bool[] tripReachable = new bool[maxTrips + 1];

    foreach (Connection conn in connections)
    {
        Console.WriteLine("Doing something...");
        if (conn.DepTime < startTime) continue;
        if (conn.DepTime > startTime + 86400) break;

        bool canTakeConnection = false;

        if (tripReachable[conn.TripId])
        {
            canTakeConnection = true;
        }
        else if (earliestArrival.TryGetValue(conn.DepStationIdx, out int arrivalTimeDepSta))
        {
            if (arrivalTimeDepSta + transferTime <= conn.DepTime)
            {
                canTakeConnection = true;
            }
        }

        if (canTakeConnection)
        {
            tripReachable[conn.TripId] = true;

            if (!earliestArrival.ContainsKey(conn.ArrStationIdx) || conn.ArrTime < earliestArrival[conn.ArrStationIdx])
            {
                earliestArrival[conn.ArrStationIdx] = conn.ArrTime;
            }
        }
    }
    return earliestArrival;
}

int Time(int hour, int minute) => (hour * 3600) + (minute * 60);

void PrintArrivalTimes(Dictionary<int, int> results)
{
    foreach (var arr in results)
    {
        string name = IdToStation[arr.Key];
        TimeSpan t = TimeSpan.FromSeconds(arr.Value);
        Console.WriteLine($"{name}: {t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}");
    }
}

int GetStationId(string stationName)
{
    if (StationToId.ContainsKey(stationName))
    {
        return StationToId[stationName];
    }

    // 2. No, this is a new station.
    // The new ID is simply the next available index (count of the list)
    int newId = IdToStation.Count; // If list has 0 items, new ID is 0.

    // 3. Save it in both places
    StationToId.Add(stationName, newId);
    IdToStation.Add(stationName);

    return newId;
}

int ParseGtfsTime(string timeString)
{
    // Handle cases where data might be null or empty
    if (string.IsNullOrEmpty(timeString)) return 0;

    // Split "08:15:30" into ["08", "15", "30"]
    var parts = timeString.Split(':');

    int h = int.Parse(parts[0]);
    int m = int.Parse(parts[1]);
    int s = (parts.Length > 2) ? int.Parse(parts[2]) : 0; // Handle missing seconds

    return (h * 3600) + (m * 60) + s;
}

int GetTripId(string rawTripId)
{
    if (TripToId.TryGetValue(rawTripId, out int id)) return id;
    int newId = TripToId.Count;
    TripToId.Add(rawTripId, newId);
    return newId;
}
public struct Connection
{
    // The index of the station in your stations array (0 to N-1)
    public int DepStationIdx;

    // The index of the station in your stations array
    public int ArrStationIdx;

    // Time in "Seconds since midnight" (or Unix timestamp)
    public int DepTime;

    // Time in "Seconds since midnight"
    public int ArrTime;

    // A unique integer ID for the specific physical vehicle run
    public int TripId;
}


// // ---------------------------------------------------------
// // SCENARIO 1: S8 (Offenbach -> Frankfurt Hbf -> Flughafen)
// // This is a direct train. The algorithm should prefer this if starting at Offenbach.
// // ---------------------------------------------------------
// rawConnections.Add(("Offenbach Marktplatz", "Frankfurt Hbf", Time(8, 00), Time(8, 15), 1));
// rawConnections.Add(("Frankfurt Hbf", "Frankfurt Flughafen", Time(8, 16), Time(8, 30), 1));
// // Note: It stops at Hbf for 1 minute (8:15 arr, 8:16 dep)

// // ---------------------------------------------------------
// // SCENARIO 2: ICE (Frankfurt Hbf -> Flughafen)
// // Leaves later than the S8, but arrives faster. 
// // If you start at Hbf at 08:10, you should wait for this ICE instead of the S8 
// // (assuming the S8 is slow or crowded, though in this data S8 arrives 8:30, ICE 8:32. 
// // Let's make the ICE faster to test "overtaking").
// // ---------------------------------------------------------
// rawConnections.Add(("Frankfurt Hbf", "Frankfurt Flughafen", Time(8, 20), Time(8, 28), 2));
// // Result: ICE arrives 8:28, S8 arrives 8:30. 
// // If you are at Hbf, taking the ICE saves 2 minutes.

// // ---------------------------------------------------------
// // SCENARIO 3: S1 (Frankfurt Hbf -> Höchst -> Wiesbaden)
// // A different branch entirely.
// // ---------------------------------------------------------
// rawConnections.Add(("Frankfurt Hbf", "Frankfurt Höchst", Time(8, 05), Time(8, 15), 3));
// rawConnections.Add(("Frankfurt Höchst", "Wiesbaden Hbf", Time(8, 16), Time(8, 40), 3));

// // ---------------------------------------------------------
// // SCENARIO 4: The Transfer (Höchst -> Hofheim)
// // You arrive at Höchst at 8:15 via S1. 
// // There is a bus leaving at 8:16. 
// // Can you catch it? (If transfer buffer is 3 mins, NO. If 0 mins, YES).
// // ---------------------------------------------------------
// rawConnections.Add(("Frankfurt Höchst", "Hofheim", Time(8, 16), Time(8, 30), 4));

// // Another bus leaves later, which you CAN catch with a 3 min transfer
// rawConnections.Add(("Frankfurt Höchst", "Hofheim", Time(8, 25), Time(8, 40), 5));