namespace Application.DTOs.Auth
{
    public class TokenValidationResultAuth
    {
        public bool IsValid { get; init; }
        public string? Message { get; init; }
        public int? UserId { get; init; }
        public string? UserName { get; init; }
    }

}