using backend.Data;
using backend.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Queries
{
    public class GetIsochronesQuery : IRequest<GeoJsonFeatureCollection>
    { }

    public class GetIsochronesHandler : IRequestHandler<GetIsochronesQuery, GeoJsonFeatureCollection>
    {
        private readonly AppDbContext _db;

        public GetIsochronesHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<GeoJsonFeatureCollection> Handle(GetIsochronesQuery request, CancellationToken cancellationToken)
        {
            var rawData = await _db.ArrivalTimes.AsNoTracking().ToListAsync(cancellationToken);

            // B. Convert to GeoJSON Feature List
            var features = rawData.Select(item => new GeoJsonFeature
            {
                Geometry = new GeoJsonGeometry
                {
                    Coordinates = new double[] { item.DestinationLocation.X, item.DestinationLocation.Y }
                },
                Properties = new Dictionary<string, object>
                {
                    { "stopId", item.DestinationStopId },
                    { "duration", item.DurationSeconds },
                    { "originId", item.OriginStopId }
                }
            }).ToList();

            return new GeoJsonFeatureCollection
            {
                Features = features
            };
        }
    }
}