using CorrePalabras.DTOs;
public interface IJwtService
{
    string GenerateToken(Guid userId, string email, int userType);
    string GenerateRefreshToken();
}
