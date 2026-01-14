using Base64Utils.Models;

namespace Base64Utils.Services
{
    public interface IBase64Service
    {
        Task<ConversionResult> EncodeFileAsync(string filePath);
        Task<string> GetFullBase64StringAsync(string filePath);
        Task<ConversionResult> DecodeBase64FileAsync(string base64FilePath);
        bool IsValidBase64(string content);
        string FormatFileSize(long bytes);
    }
}
