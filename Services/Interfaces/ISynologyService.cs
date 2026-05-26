namespace CorrePalabras.Services.Interfaces
{
    public interface ISynologyService
    {
        Task<string> UploadAndShareAsync(IFormFile file, string destinationFolder, string fileName);
        Task DeleteBySharingUrlAsync(string sharingUrl);
    }
}