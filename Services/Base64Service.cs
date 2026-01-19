using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Base64Utils.Models;

namespace Base64Utils.Services
{
    public class Base64Service : IBase64Service
    {
        private const int PreviewLength = 100;
        private readonly IFileTypeDetectionService _fileTypeDetectionService;
        private readonly IFileService _fileService;

        public Base64Service(IFileTypeDetectionService fileTypeDetectionService, IFileService fileService)
        {
            _fileTypeDetectionService = fileTypeDetectionService;
            _fileService = fileService;
        }

        public async Task<ConversionResult> EncodeFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new ConversionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "File does not exist. Please check the path and try again."
                    };
                }

                var fileInfo = new FileInfo(filePath);
                var fileSize = fileInfo.Length;

                // Generate preview (first ~100 characters of Base64)
                string previewBase64;
                using (FileStream inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (CryptoStream base64Stream = new CryptoStream(inputFile, new ToBase64Transform(), CryptoStreamMode.Read))
                {
                    byte[] buffer = new byte[PreviewLength];
                    int bytesRead = await base64Stream.ReadAsync(buffer, 0, PreviewLength);
                    previewBase64 = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }

                // Calculate estimated full Base64 length
                long estimatedBase64Length = ((fileSize + 2) / 3) * 4;

                return new ConversionResult
                {
                    IsSuccess = true,
                    OriginalSize = fileSize,
                    ConvertedSize = estimatedBase64Length,
                    Preview = estimatedBase64Length > PreviewLength 
                        ? $"{previewBase64}... [Truncated]" 
                        : previewBase64
                };
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error converting file, please check the file content and access permissions and try again: {ex.Message}"
                };
            }
        }

        public async Task<string> GetFullBase64StringAsync(string filePath)
        {
            using (FileStream inputFile = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (MemoryStream outputStream = new MemoryStream())
            using (CryptoStream base64Stream = new CryptoStream(outputStream, new ToBase64Transform(), CryptoStreamMode.Write))
            {
                await inputFile.CopyToAsync(base64Stream);
                base64Stream.FlushFinalBlock();
                return System.Text.Encoding.ASCII.GetString(outputStream.ToArray());
            }
        }

        public async Task<ConversionResult> DecodeBase64FileAsync(string base64FilePath)
        {
            try
            {
                if (!File.Exists(base64FilePath))
                {
                    return new ConversionResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "File does not exist. Please check the path and try again."
                    };
                }

                var fileInfo = new FileInfo(base64FilePath);
                var base64FileSize = fileInfo.Length;

                // Read the Base64 content from the file
                string base64Content = await File.ReadAllTextAsync(base64FilePath);

                // Decode Base64 to binary
                byte[] decodedBytes = Convert.FromBase64String(base64Content);

                // Create a temporary file for the decoded content (without extension first)
                string tempFilePathWithoutExt = Path.Combine(Path.GetTempPath(), $"decoded_{Guid.NewGuid()}");
                
                // Write the decoded binary content to the temporary file
                await File.WriteAllBytesAsync(tempFilePathWithoutExt, decodedBytes);

                // Detect file type
                var (fileType, extension) = await _fileTypeDetectionService.DetectFileTypeAsync(tempFilePathWithoutExt);

                // Rename the file with the proper extension
                string tempFilePath = $"{tempFilePathWithoutExt}.{extension}";
                File.Move(tempFilePathWithoutExt, tempFilePath);

                // Track the temporary file for cleanup on exit
                _fileService.TrackTemporaryFile(tempFilePath);

                var decodedFileSize = new FileInfo(tempFilePath).Length;

                return new ConversionResult
                {
                    IsSuccess = true,
                    OriginalSize = base64FileSize,
                    ConvertedSize = decodedFileSize,
                    TemporaryFilePath = tempFilePath,
                    FileType = fileType,
                    FileExtension = extension
                };
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error decoding from Base64, please check the inputs and try again: {ex.Message}"
                };
            }
        }

        public bool IsValidBase64(string content)
        {
            // Remove whitespace and newlines
            string cleaned = Regex.Replace(content, @"\s+", "");

            // Base64 should only contain A-Z, a-z, 0-9, +, /, and = (padding)
            if (!Regex.IsMatch(cleaned, @"^[A-Za-z0-9+/]*={0,2}$"))
            {
                return false;
            }

            // Length should be a multiple of 4
            if (cleaned.Length % 4 != 0)
            {
                return false;
            }

            // Try to decode a small portion to validate
            try
            {
                // Take only the first valid Base64 block for validation
                int blockSize = Math.Min(4, cleaned.Length);
                string testBlock = cleaned.Substring(0, blockSize);
                while (testBlock.Length < 4)
                {
                    testBlock += "A"; // Pad with valid Base64 character for testing
                }
                Convert.FromBase64String(testBlock);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string FormatFileSize(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes >= GB)
            {
                return $"{(double)bytes / GB:F2} GB";
            }
            else if (bytes >= MB)
            {
                return $"{(double)bytes / MB:F2} MB";
            }
            else if (bytes >= KB)
            {
                return $"{(double)bytes / KB:F2} KB";
            }
            else
            {
                return $"{bytes} bytes";
            }
        }
    }
}
