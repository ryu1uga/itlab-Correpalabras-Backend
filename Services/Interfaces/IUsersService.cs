using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IUsersService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetUserProfilesAsync(Guid userId);
        Task<int> GetTotalCountAsync();
        Task<string> CreateAsync(UserRequest dto);
        Task<string> UpdateAsync(Guid id, UserRequest dto);
        Task<string> UpdateRoleAsync(Guid id, int userType);
        Task<string> DeleteAsync(EmailVerificationRequest dto);
        Task<object?> LoginAsync(LoginRequest dto, bool isAdmin);
        Task<object?> RefreshTokenAsync(RefreshTokenRequest dto);
        Task<string> LogoutAsync(Guid id);
        Task<string> GenerateVerificationCodeAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordRequest dto);
    }
}