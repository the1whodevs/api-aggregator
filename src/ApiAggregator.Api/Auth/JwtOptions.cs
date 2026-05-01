using System.Text;

namespace ApiAggregator.Api.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;

    public void Validate() {
        if (string.IsNullOrWhiteSpace(Issuer)) {
            throw new InvalidOperationException("Jwt:Issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(Audience)) {
            throw new InvalidOperationException("Jwt:Audience must be configured.");
        }

        if (Encoding.UTF8.GetByteCount(SecretKey) < 32) {
            throw new InvalidOperationException("Jwt:SecretKey must be at least 32 bytes long.");
        }

        if (ExpirationMinutes <= 0) {
            throw new InvalidOperationException("Jwt:ExpirationMinutes must be greater than zero.");
        }
    }
}
