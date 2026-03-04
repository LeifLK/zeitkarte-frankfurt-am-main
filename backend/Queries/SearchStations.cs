using backend.DTOs;
using MediatR;
using Npgsql;

namespace backend.Queries;

public class SearchStationsQuery(string searchTerm) : IRequest<List<StationDto>>
{
    public string SearchTerm { get; } = searchTerm;
}

public class SearchStationsHandler : IRequestHandler<SearchStationsQuery, List<StationDto>>
{
    private readonly NpgsqlDataSource _dataSource;

    public SearchStationsHandler(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<StationDto>> Handle(SearchStationsQuery request, CancellationToken cancellationToken)
    {
        List<StationDto> stations = new List<StationDto>();

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return stations;
        }

        string sql = @"
            WITH ActiveGroupedStops AS (
                SELECT 
                    COALESCE(NULLIF(parent_station, ''), stop_id) AS group_id, 
                    MIN(TRIM(REGEXP_REPLACE(stop_name, '^F[\s\-]+', ''))) AS display_name
                FROM stops
                WHERE stop_name ILIKE @searchTerm
                AND ST_DWithin(
                    geom::geography, 
                    ST_SetSRID(ST_MakePoint(8.682, 50.110), 4326)::geography, 
                    15000
                )
                -- Only keep platforms that actually have Monday trips
                AND EXISTS (
                    SELECT 1 
                    FROM stop_times st
                    JOIN trips t ON st.trip_id = t.trip_id
                    JOIN calendar c ON t.service_id = c.service_id
                    WHERE st.stop_id = stops.stop_id
                    AND c.monday = '1'
                )
                GROUP BY COALESCE(NULLIF(parent_station, ''), stop_id)
                LIMIT 10
            )
            SELECT 
                a.group_id AS stop_id,
                a.display_name,
                -- Grab the exact coordinates of the Parent Station row
                ST_Y(s.geom) AS lat,
                ST_X(s.geom) AS lon
            FROM ActiveGroupedStops a
            JOIN stops s ON a.group_id = s.stop_id
            ORDER BY a.display_name;
        ";

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("searchTerm", $"%{request.SearchTerm}%");

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            stations.Add(new StationDto(
                Id: reader.GetString(0),
                DisplayName: reader.GetString(1),
                Lat: reader.GetDouble(2),
                Lon: reader.GetDouble(3)
            ));
            Console.WriteLine(reader.GetString(0));
            Console.WriteLine(reader.GetString(1));
            Console.WriteLine(reader.GetDouble(2));
            Console.WriteLine(reader.GetDouble(3));

        }

        Console.WriteLine(stations);
        return stations;
    }
}
