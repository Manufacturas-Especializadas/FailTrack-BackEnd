using System;
using System.Collections.Generic;

namespace FailTrack.Models;

public partial class Status
{
    public int Id { get; set; }

    public string StatusName { get; set; }

    public virtual ICollection<Maintenance> Maintenance { get; set; } = new List<Maintenance>();
}