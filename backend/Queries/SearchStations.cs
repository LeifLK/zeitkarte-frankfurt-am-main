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
            SELECT 
                stop_id, 
                TRIM(REGEXP_REPLACE(stop_name, '^F[\s\-]+', '')) AS display_name,
                ST_Y(geom) AS lat,
                ST_X(geom) AS lon
            FROM stops
            WHERE stop_name ILIKE @searchTerm
            AND ST_DWithin(
                geom::geography, 
                ST_SetSRID(ST_MakePoint(8.682, 50.110), 4326)::geography, 
                15000
            )
            LIMIT 10;
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
