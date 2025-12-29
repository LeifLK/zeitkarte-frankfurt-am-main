using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace backend.Entities;

[Table("arrival_times")]
public class ArrivalTimeEntity
{
    [Column("origin_stop_id")]
    public required string OriginStopId { get; set; }

    [Column("dest_stop_id")]
    public required string DestinationStopId { get; set; }

    [Column("duration_seconds")]
    public int DurationSeconds { get; set; }

    [Column("dest_geom")]
    public required Point DestinationLocation { get; set; }
}