namespace FailTrack.Dtos
{
    public class MaintenanceDto
    {
        public string ApplicantName { get; set; } = string.Empty;

        public string? FaultDescription { get; set; }

        public string? LineFaultDescription { get; set; }

        public int? IdLine { get; set; }

        public int? IdMachine { get; set; }

        public int? IdStatus { get; set; }

        public string? Responsible { get; set; }

        public string? FailureSolution { get; set; }

        public DateTime? ClosingDate { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}