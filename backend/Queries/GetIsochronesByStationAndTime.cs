using System.Text.Json;
using Mediator;
using Npgsql;

namespace backend.Queries;

public class GetIsochronesByStationAndTimeQuery(int station, int duration) : IRequest<JsonDocument>
{
    public int Station { get; } = station;
    public int Duration { get; } = duration;
}

public class GetIsochronesByStationAndTimeHandler : IRequestHandler<GetIsochronesByStationAndTimeQuery, JsonDocument>
{
    private readonly NpgsqlDataSource _dataSource;

    public GetIsochronesByStationAndTimeHandler(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async ValueTask<JsonDocument> Handle(GetIsochronesByStationAndTimeQuery request, CancellationToken cancellationToken)
    {
        string sql = $@"
        /* * Generates an isochrone by buffering reachable stations.
        * Radius = remaining time * 1.0 m/s (adjusted for urban walking).
        * Walk time is capped at 15 mins (900s), with a 50m minimum radius.
        */
        SELECT ST_AsGeoJSON(
            ST_Simplify(
                ST_Union(
                    ST_Buffer(
                        dest_geom::geography, 
                        GREATEST(
                            LEAST(@duration - duration_seconds, 900 ) * 1.0,
                            50
                        ),
                        'quad_segs=16'
                    )::geometry
                ), 
                0.00005
            )            
        )
        FROM arrival_times a
		WHERE origin_stop_id = @stationId
		AND duration_seconds < @duration
        ";

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("stationId", request.Station);
        cmd.Parameters.AddWithValue("duration", request.Duration);

        var jsonString = await cmd.ExecuteScalarAsync(cancellationToken) as string;

        if (string.IsNullOrEmpty(jsonString))
        {
            return JsonDocument.Parse("{ \"type\": \"FeatureCollection\", \"features\": [] }");
        }

        return JsonDocument.Parse(jsonString);
    }
}