using CorrePalabras.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CorrePalabras.Services.Interfaces
{
    public interface IUsersService
    {
        Task<IEnumerable<object>> GetAllAsync();
        Task<object?> GetByIdAsync(Guid id);
        Task<IEnumerable<object>> GetUserProfilesAsync(Guid userId);
        Task<int> GetTotalCountAsync();
        Task<string> CreateAsync(UserDTO dto);
        Task<string> UpdateAsync(Guid id, UserDTO dto);
        Task<string> DeleteAsync(EmailVerificationDTO dto);
        Task<object?> LoginAsync(LoginRequestDTO dto, bool isAdmin);
        Task<string> LogoutAsync(Guid id);
        Task<string> GenerateVerificationCodeAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordDTO dto);
    }
}