using Microsoft.Win32;
using System.IO;

namespace Base64Utils.Services
{
    public class FileService : IFileService
    {
        private static readonly List<string> _temporaryFiles = new List<string>();
        private static readonly object _lockObject = new object();
        public string? OpenFileDialog(string title, string filter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        public string? SaveFileDialog(string title, string filter, string defaultFileName)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                DefaultExt = Path.GetExtension(defaultFileName),
                FileName = defaultFileName
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }

        public async Task SaveTextToFileAsync(string filePath, string content)
        {
            await File.WriteAllTextAsync(filePath, content);
        }

        public async Task CopyFileAsync(string sourceFilePath, string destinationFilePath)
        {
            await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, overwrite: true));
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to delete file: {ex.Message}");
                }
            }
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public void TrackTemporaryFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                lock (_lockObject)
                {
                    if (!_temporaryFiles.Contains(filePath))
                    {
                        _temporaryFiles.Add(filePath);
                    }
                }
            }
        }

        public static void CleanupAllTemporaryFiles()
        {
            lock (_lockObject)
            {
                foreach (var filePath in _temporaryFiles)
                {
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath);
                            System.Diagnostics.Debug.WriteLine($"Deleted temporary file: {filePath}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to delete temporary file {filePath}: {ex.Message}");
                        }
                    }
                }
                _temporaryFiles.Clear();
            }
        }
    }
}
