using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace backend.Entities;

[Table("stops")]
public class StopEntity
{
    [Column("stop_id")]
    public required string StopId { get; set; }

    [Column("stop_name")]
    public required string Name { get; set; }

    [Column("geom")]
    public required Point Location { get; set; }
}
