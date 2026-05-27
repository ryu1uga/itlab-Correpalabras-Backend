using CorrePalabras.Data;
using CorrePalabras.DTOs.Common;
using CorrePalabras.Models.Common;
using CorrePalabras.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CorrePalabras.Services
{
    public class UsersService : IUsersService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IJwtService _jwtService;

        public UsersService(ApplicationDbContext context, EmailService emailService, IJwtService jwtService)
        {
            _context = context;
            _emailService = emailService;
            _jwtService = jwtService;
        }

        public async Task<IEnumerable<object>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Profiles).ThenInclude(p => p.Avatar)
                .OrderBy(u => u.Id)
                .Select(u => new {
                    u.Id, u.Name, u.Email, u.UserType,
                    Profiles = u.Profiles.Select(p => new {
                        p.Id, p.AvatarId, AvatarUrl = p.Avatar.AvatarUrl, p.Username, p.Gender, p.BirthDate
                    }).ToList()
                }).ToListAsync();
        }

        public async Task<object?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Profiles).ThenInclude(p => p.Avatar)
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id, u.Name, u.Email,
                    Profiles = u.Profiles.Select(p => new {
                        p.Id, p.AvatarId, AvatarUrl = p.Avatar.AvatarUrl, p.Username, p.Gender, p.BirthDate
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetUserProfilesAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Profiles).ThenInclude(p => p.Avatar)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null!;

            return user.Profiles.OrderBy(p => p.Id).Select(p => new {
                p.Id, p.AvatarId, AvatarUrl = p.Avatar.AvatarUrl, p.Username, p.Gender, p.BirthDate
            }).ToList();
        }

        public async Task<int> GetTotalCountAsync() => await _context.Users.CountAsync();

        public async Task<string> CreateAsync(UserDTO dto)
        {
            // Validación de email
            ValidateEmail(dto.Email);
            
            // Validación de password
            ValidatePassword(dto.Password);
            
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("El email ya se encuentra en uso.");

            var user = new User {
                Id = Guid.NewGuid(), Name = dto.Name, Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UpdatedAt = DateTime.UtcNow, UserType = 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return "Usuario creado correctamente.";
        }

        public async Task<string> UpdateAsync(Guid id, UserDTO dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new KeyNotFoundException();

            if (user.Email != dto.Email && await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("El email ya se encuentra en uso.");

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.UserType = dto.UserType;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return "Valores actualizados correctamente.";
        }

        public async Task<string> DeleteAsync(EmailVerificationDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) throw new KeyNotFoundException();

            if (user.VerificationCode != dto.Code || user.CodeExpirationDate <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("El código ingresado no es válido o ha expirado.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return "Usuario eliminado correctamente.";
        }

        public async Task<object?> LoginAsync(LoginRequestDTO dto, bool isAdmin)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) throw new KeyNotFoundException("El email ingresado no existe.");
            if (isAdmin && user.UserType != 1) throw new UnauthorizedAccessException("El usuario no es administrador.");
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)) throw new UnauthorizedAccessException("Contraseña incorrecta.");

            var token = _jwtService.GenerateToken(user.Id, user.Email, user.UserType);

            return new { user.Id, user.Name, user.Email, Token = token };
        }

        public async Task<string> LogoutAsync(Guid id)
        {
            return "Cierre de sesión realizado exitosamente.";
        }

        public async Task<string> GenerateVerificationCodeAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new KeyNotFoundException("El email ingresado no existe.");

            user.VerificationCode = new Random().Next(1000, 10000);
            user.CodeRegisteredDate = DateTime.UtcNow;
            user.CodeExpirationDate = DateTime.UtcNow.AddDays(1);

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(user.Email, "Código de verificación", 
                $"<p>Hola {user.Name},</p><p>Tu código es: <strong>{user.VerificationCode}</strong></p>");

            return "Código de verificación enviado al email.";
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) throw new KeyNotFoundException();
            if (user.VerificationCode != dto.Code || user.CodeExpirationDate <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Código no válido.");
            if (dto.Password != dto.ConfirmPassword) throw new ArgumentException("Las contraseñas no coinciden.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.VerificationCode = null;
            await _context.SaveChangesAsync();
            return "Contraseña actualizada correctamente.";
        }

        // Validar formato de email
        private void ValidateEmail(string email)
        {
            const string emailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
            if (!Regex.IsMatch(email, emailPattern))
                throw new ArgumentException("El formato del email no es válido.");
        }

        // Validar contraseña (mínimo 8 caracteres, mayúscula, minúscula, número y carácter especial)
        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            if (password.Length < 8)
                throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");

            // Mínimo 1 mayúscula
            if (!Regex.IsMatch(password, @"[A-Z]"))
                throw new ArgumentException("La contraseña debe contener al menos una mayúscula.");

            // Mínimo 1 minúscula
            if (!Regex.IsMatch(password, @"[a-z]"))
                throw new ArgumentException("La contraseña debe contener al menos una minúscula.");

            // Mínimo 1 número
            if (!Regex.IsMatch(password, @"[0-9]"))
                throw new ArgumentException("La contraseña debe contener al menos un número.");
        }
    }
}