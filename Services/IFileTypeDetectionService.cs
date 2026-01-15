namespace Base64Utils.Services
{
    public interface IFileTypeDetectionService
    {
        /// <summary>
        /// Detects the file type and extension from a file path.
        /// </summary>
        /// <param name="filePath">Path to the file to analyze</param>
        /// <returns>A tuple containing the file type description and extension (without dot)</returns>
        Task<(string FileType, string Extension)> DetectFileTypeAsync(string filePath);
    }
}
