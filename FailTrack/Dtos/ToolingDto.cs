namespace FailTrack.Dtos
{
    public class ToolingDto
    {
        public string ApplicantName { get; set; } = string.Empty;

        public string FaultDescription { get; set; } = string.Empty;

        public int? IdLine { get; set; }

        public int? IdMachine { get; set; }

        public int? IdStatus { get; set; }

        public string Responsible { get; set; } = string.Empty;

        public string FailureSolution { get; set; } = string.Empty;
    }
}