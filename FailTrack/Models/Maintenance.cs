using System;
using System.Collections.Generic;

namespace FailTrack.Models;

public partial class Maintenance
{
    public int Id { get; set; }

    public string? ApplicantName { get; set; }

    public string? FaultDescription { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? IdLine { get; set; }

    public int? IdMachine { get; set; }

    public int? IdStatus { get; set; }

    public string? Responsible { get; set; }

    public string? FailureSolution { get; set; }

    public DateTime? ClosingDate { get; set; }

    public virtual Lines IdLineNavigation { get; set; }

    public virtual Machines IdMachineNavigation { get; set; }

    public virtual Status IdStatusNavigation { get; set; }
}