using System.Threading.Tasks;
using CorrePalabras.DTOs;

namespace CorrePalabras.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}