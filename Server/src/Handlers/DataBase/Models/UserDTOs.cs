namespace Server.Handlers.DataBase.Models;

public class UserDtOs
{
    public record RegisterRequest
    {
        public string? Email { get; init; }
        public string? Username { get; init; }
        public string? Password { get; init; }
    }

    public record RegisterResponse
    {
        public int UserId { get; init; }
        public Guid SessionId { get; init; }
        public string? SuccessMessage { get; init; }
    }

    public record LoginRequest
    {
        public string? UsernameOrEmail { get; init; }
        public string? Password { get; init; }
    }

    public record LoginResponse
    {
        public int UserId { get; init; }
        public Guid SessionId { get; init; }
        public string? Username { get; init; }
        public string? SuccessMessage { get; init; }
    }
    
    public record LogoutRequest
    {
        public Guid SessionId { get; init; }
    }

    public record LogoutResponse
    {
        public string? SuccessMessage { get; init; }
    }

}
