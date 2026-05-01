namespace ApiAggregator.Api.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresInSeconds { get; init; }
}
