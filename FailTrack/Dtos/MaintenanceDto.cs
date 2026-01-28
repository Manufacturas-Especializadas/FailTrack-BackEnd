namespace FailTrack.Dtos
{
    public class MaintenanceDto
    {
        public string ApplicantName { get; set; } = string.Empty;

        public string FaultDescription { get; set; } = string.Empty;

        public int? IdLine { get; set; }

        public int? IdMachine { get; set; }

        public int? IdStatus { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}