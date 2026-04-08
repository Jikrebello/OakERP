namespace OakERP.Auth.Jwt;

public sealed class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpireMinutes { get; set; } = 60;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Key) || Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:Key must be at least 32 characters for HMAC-SHA256."
            );
        }

        if (string.IsNullOrWhiteSpace(Issuer))
        {
            throw new InvalidOperationException("JwtSettings:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(Audience))
        {
            throw new InvalidOperationException("JwtSettings:Audience is required.");
        }

        if (ExpireMinutes <= 0)
        {
            throw new InvalidOperationException("JwtSettings:ExpireMinutes must be greater than 0.");
        }
    }
}
