namespace FailTrack.Dtos
{
    public class UserRegisterDto
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int? IdLine { get; set; }
    }
}