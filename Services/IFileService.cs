namespace Base64Utils.Services
{
    public interface IFileService
    {
        string? OpenFileDialog(string title, string filter);
        string? SaveFileDialog(string title, string filter, string defaultFileName);
        Task SaveTextToFileAsync(string filePath, string content);
        Task CopyFileAsync(string sourceFilePath, string destinationFilePath);
        void DeleteFile(string filePath);
        bool FileExists(string filePath);
    }
}
