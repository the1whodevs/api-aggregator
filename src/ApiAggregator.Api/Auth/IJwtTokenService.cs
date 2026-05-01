namespace ApiAggregator.Api.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(string username);
}
