using System;
using System.Collections.Generic;

namespace FailTrack.Models;

public partial class Lines
{
    public int Id { get; set; }

    public string LineName { get; set; }

    public virtual ICollection<Machines> Machines { get; set; } = new List<Machines>();

    public virtual ICollection<Maintenance> Maintenance { get; set; } = new List<Maintenance>();

    public virtual ICollection<Tooling> Tooling { get; set; } = new List<Tooling>();
}