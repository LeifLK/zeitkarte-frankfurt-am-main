using Npgsql;

// not used???
var rawConnections = new List<(string from, string to, int dep, int arr, int tripId)>();

// StationId from gtfs datat to internal id of script
Dictionary<int, int> GtfsStationIdToId = new Dictionary<int, int>();

// Converts script id to gtfs id
List<int> IdToGtfsStationId = new List<int>();

List<Connection> tempConnList = new List<Connection>();

// Is this used other then for passing the number of trips to csa?
Dictionary<string, int> TripToId = new Dictionary<string, int>();

var connectionString = "Host=localhost;Username=leif;Password=;Database=zeitkarte_ffm";
await using var dataSource = NpgsqlDataSource.Create(connectionString);

string sql = @"
    SELECT 
        A.stop_id as departure_station_id, 
        B.stop_id as arrival_station_id, 
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
    ORDER BY A.departure_time ASC
";

await using var command = dataSource.CreateCommand(sql);

command.CommandTimeout = 0;

await using var reader = await command.ExecuteReaderAsync();

while (await reader.ReadAsync())
{
    int fromStop = int.Parse(reader.GetString(0));
    int toStop = int.Parse(reader.GetString(1));
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

//Dictionary<int, int> earliestArrival = new Dictionary<int, int>();

await using var dbConn = await dataSource.OpenConnectionAsync();

// clear table
await using var truncateCommand = new NpgsqlCommand("TRUNCATE arrival_times", dbConn);
await truncateCommand.ExecuteNonQueryAsync();

// could use Parallel.ForEach to make ist super fast with lock for db access of different threads
for (int i = 0; i < IdToGtfsStationId.Count; i++)
{
    Dictionary<int, int> earliestArrival = csa(connections, IdToGtfsStationId[i], Time(7, 50), TripToId.Count);
    //PrintArrivalTimes(earliestArrival);

    // Import earliestArrval results in to the database arrival_times
    await using (var writer = await dbConn.BeginBinaryImportAsync(
        "COPY arrival_times (origin_stop_id, dest_stop_id, duration_seconds) FROM STDIN (FORMAT BINARY)"))
    {
        foreach (var kvp in earliestArrival)
        {
            int ArrivalstationGtfsId = IdToGtfsStationId[kvp.Key];
            int arrivalTime = kvp.Value;

            int duration = arrivalTime - Time(7, 50);

            await writer.StartRowAsync();
            await writer.WriteAsync(IdToGtfsStationId[i]);
            await writer.WriteAsync(ArrivalstationGtfsId);
            await writer.WriteAsync(duration);
        }
        await writer.CompleteAsync();
    }

    int progress = i * 100 / GtfsStationIdToId.Count;

    Console.Write($"\rProgress: {progress}% ({i}/{GtfsStationIdToId.Count})");
}

// fill dest_geom column in arrival_times from table stops
string updateSql = @"
    UPDATE arrival_times a
    SET dest_geom = s.geom
    FROM stops s
    WHERE a.dest_stop_id = s.stop_id::integer;
    ";

await using var updateCmd = new NpgsqlCommand(updateSql, dbConn);
updateCmd.CommandTimeout = 0;
await updateCmd.ExecuteNonQueryAsync();


// -----------------------------------------------------------------------------------------------------------------


Dictionary<int, int> csa(Connection[] connections, int startStation, int startTime, int maxTrips)
{
    int transferTime = 180;
    Dictionary<int, int> earliestArrival = new Dictionary<int, int>();
    earliestArrival.Add(GtfsStationIdToId[startStation], startTime);

    bool[] tripReachable = new bool[maxTrips + 1];

    foreach (Connection conn in connections)
    {
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
        int name = IdToGtfsStationId[arr.Key];
        TimeSpan t = TimeSpan.FromSeconds(arr.Value);
        Console.WriteLine($"{name}: {t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}");
    }
}

int GetStationId(int stationGtfsId)
{
    if (GtfsStationIdToId.ContainsKey(stationGtfsId))
    {
        return GtfsStationIdToId[stationGtfsId];
    }

    int newId = IdToGtfsStationId.Count;

    GtfsStationIdToId.Add(stationGtfsId, newId);
    IdToGtfsStationId.Add(stationGtfsId);

    return newId;
}

int ParseGtfsTime(string timeString)
{
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
    public int DepStationIdx;

    public int ArrStationIdx;

    public int DepTime;

    public int ArrTime;

    public int TripId;
}