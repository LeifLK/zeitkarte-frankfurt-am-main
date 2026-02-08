using MediatR;
using Npgsql;
using System.Text.Json;

namespace backend.Queries;

public class GetBlobQuery : IRequest<JsonDocument>
{ }

public class GetBlobHandler : IRequestHandler<GetBlobQuery, JsonDocument>
{
    private readonly NpgsqlDataSource _dataSource;

    public GetBlobHandler(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<JsonDocument> Handle(GetBlobQuery request, CancellationToken cancellationToken)
    {
        string sql = @"
        SELECT ST_AsGeoJSON(
            ST_Simplify(
                ST_Union(
                    ST_Buffer(dest_geom::geography, 500)::geometry
                ), 
                0.0001
            )            
        )
        FROM arrival_times a
        ";

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);

        var jsonString = await cmd.ExecuteScalarAsync(cancellationToken) as string;

        if (string.IsNullOrEmpty(jsonString))
        {
            return JsonDocument.Parse("{ \"type\": \"FeatureCollection\", \"features\": [] }");
        }

        return JsonDocument.Parse(jsonString);
    }
}