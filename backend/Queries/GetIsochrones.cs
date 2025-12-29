using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs;
using backend.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace backend.Queries
{
    public class GetAllRawQuery : IRequest<List<object>>
    {
    }

    public class GetAllRawHandler : IRequestHandler<GetAllRawQuery, List<object>>
    {
        private readonly AppDbContext _db;

        public GetAllRawHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<object>> Handle(GetAllRawQuery request, CancellationToken cancellationToken)
        {
            var query = from arrivalTime in _db.ArrivalTimes
                        select new
                        {
                            OriginId = arrivalTime.OriginStopId,
                            DestId = arrivalTime.DestinationStopId,
                            Duration = arrivalTime.DurationSeconds,
                            StopName = arrivalTime.DestinationStopId,
                            Lat = arrivalTime.DestinationLocation.Y,
                            Lon = arrivalTime.DestinationLocation.X
                        };

            var result = await query.ToListAsync(cancellationToken);
            return result.Cast<object>().ToList();
        }
    }
}