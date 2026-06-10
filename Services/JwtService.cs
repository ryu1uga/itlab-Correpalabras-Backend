using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class JwtService : IJwtService
{
    public JwtService() { }

    public string GenerateToken(Guid userId, string email, int userType)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? throw new ArgumentNullException("JWT_KEY no configurada");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new ArgumentNullException("JWT_ISSUER no configurada");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new ArgumentNullException("JWT_AUDIENCE no configurada");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),  // ℹ️ Claim estándar para userId
            new Claim(ClaimTypes.Email, email),
            new Claim("userType", userType.ToString()), // Claim personalizado
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
