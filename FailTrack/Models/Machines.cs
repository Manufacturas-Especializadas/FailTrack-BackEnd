using System;
using System.Collections.Generic;

namespace FailTrack.Models;

public partial class Machines
{
    public int Id { get; set; }

    public int? IdLine { get; set; }

    public string MachineName { get; set; }

    public virtual Lines IdLineNavigation { get; set; }

    public virtual ICollection<Maintenance> Maintenance { get; set; } = new List<Maintenance>();

    public virtual ICollection<Tooling> Tooling { get; set; } = new List<Tooling>();
}