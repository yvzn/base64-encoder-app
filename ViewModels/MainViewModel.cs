using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Base64Utils.Models;
using Base64Utils.Services;

namespace Base64Utils.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IBase64Service _base64Service;
        private readonly IFileService _fileService;

        // Brushes for different modes
        private static readonly SolidColorBrush EncodeAccentBrush = new SolidColorBrush(Color.FromRgb(64, 162, 227));
        private static readonly SolidColorBrush DecodeAccentBrush = new SolidColorBrush(Color.FromRgb(13, 146, 118));
        private static readonly SolidColorBrush ErrorBrush = new SolidColorBrush(Color.FromRgb(204, 0, 0));
        private static readonly SolidColorBrush NeutralBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51));

        public MainViewModel(IBase64Service base64Service, IFileService fileService)
        {
            _base64Service = base64Service;
            _fileService = fileService;
            UpdateStatusMessage("Ready in Encode mode.");
            ShowcasedButton = "SelectFile";
        }

        #region Observable Properties

        [ObservableProperty]
        private AppMode _currentMode = AppMode.Encode;

        [ObservableProperty]
        private string? _selectedFilePath;

        [ObservableProperty]
        private string _fileNameDisplay = "No file selected.";

        [ObservableProperty]
        private bool _isConvertButtonEnabled;

        [ObservableProperty]
        private bool _isCopyButtonEnabled;

        [ObservableProperty]
        private bool _isSaveToFileButtonEnabled;

        [ObservableProperty]
        private bool _isEncodeResultVisible;

        [ObservableProperty]
        private string _encodeResultInfo = string.Empty;

        [ObservableProperty]
        private string _encodePreview = string.Empty;

        [ObservableProperty]
        private string? _selectedBase64FilePath;

        [ObservableProperty]
        private string _base64FileNameDisplay = "No file selected.";

        [ObservableProperty]
        private bool _isDecodeButtonEnabled;

        [ObservableProperty]
        private bool _isSaveDecodedButtonEnabled;

        [ObservableProperty]
        private bool _isDecodeResultVisible;

        [ObservableProperty]
        private string _decodeResultInfo = string.Empty;

        [ObservableProperty]
        private string _base64PastePreview = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private SolidColorBrush _statusForeground = NeutralBrush;

        [ObservableProperty]
        private SolidColorBrush _accentBrush = EncodeAccentBrush;

        [ObservableProperty]
        private bool _isProcessing;

        [ObservableProperty]
        private int _selectedTabIndex;

        [ObservableProperty]
        private string? _showcasedButton;

        [ObservableProperty]
        private bool _isStatusPopupOpen;

        [ObservableProperty]
        private string _fullStatusMessage = "Ready";

        [ObservableProperty]
        private bool _canExpandStatusMessage;

        [ObservableProperty]
        private bool _shouldFocusStatusBar;

        [ObservableProperty]
        private string _copyButtonText = "Copy to Clipboard";

        [ObservableProperty]
        private string _saveToFileButtonText = "Save to File";

        [ObservableProperty]
        private string _pasteBase64ButtonText = "Paste from Clipboard";

        [ObservableProperty]
        private string _saveDecodedButtonText = "Save to File";

        private long _originalFileSize;
        private string? _decodedTemporaryFilePath;
        private string? _pastedBase64TemporaryFilePath;

        #endregion

        #region Commands

        [RelayCommand]
        private void SelectFile()
        {
            var filePath = _fileService.OpenFileDialog(
                "Select a file to convert to Base64",
                "All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(filePath))
            {
                SelectedFilePath = filePath;
                FileNameDisplay = $"Selected file: {Path.GetFileName(filePath)}";
                IsConvertButtonEnabled = true;
                IsEncodeResultVisible = false;
                IsCopyButtonEnabled = false;
                IsSaveToFileButtonEnabled = false;
                _originalFileSize = 0;
                ShowcasedButton = "Convert";
            }
            else
            {
                SelectedFilePath = null;
                FileNameDisplay = "No file selected.";
                IsConvertButtonEnabled = false;
                IsEncodeResultVisible = false;
                IsCopyButtonEnabled = false;
                IsSaveToFileButtonEnabled = false;
                _originalFileSize = 0;
                ShowcasedButton = "SelectFile";
            }
        }

        [RelayCommand]
        private async Task ConvertToBase64Async()
        {
            if (string.IsNullOrEmpty(SelectedFilePath) || !_fileService.FileExists(SelectedFilePath))
            {
                UpdateStatusMessage("Please select a valid file and try again.", isError: true);
                return;
            }

            UpdateStatusMessage("Converting file to Base64...");
            IsProcessing = true;
            IsConvertButtonEnabled = false;
            IsCopyButtonEnabled = false;
            IsSaveToFileButtonEnabled = false;
            IsEncodeResultVisible = false;
            ShowcasedButton = null;

            try
            {
                var result = await _base64Service.EncodeFileAsync(SelectedFilePath);

                if (result.IsSuccess)
                {
                    _originalFileSize = result.OriginalSize;
                    EncodeResultInfo = $"Original file size: {_base64Service.FormatFileSize(result.OriginalSize)}\n" +
                                      $"Base64 output size: {result.ConvertedSize:N0} characters";
                    EncodePreview = result.Preview ?? string.Empty;
                    IsEncodeResultVisible = true;
                    IsCopyButtonEnabled = true;
                    IsSaveToFileButtonEnabled = true;
                    UpdateStatusMessage($"Conversion complete. Generated {result.ConvertedSize:N0} characters.");
                    ShowcasedButton = "Copy";
                }
                else
                {
                    UpdateStatusMessage(result.ErrorMessage ?? "Unknown error occurred.", isError: true);
                    _originalFileSize = 0;
                    IsCopyButtonEnabled = false;
                    IsSaveToFileButtonEnabled = false;
                    IsEncodeResultVisible = false;
                    ShowcasedButton = "Convert";
                }
            }
            catch (Exception ex)
            {
                UpdateStatusMessage($"Error: {ex.Message}", isError: true);
            }
            finally
            {
                IsProcessing = false;
                IsConvertButtonEnabled = true;
            }
        }

        [RelayCommand]
        private async Task CopyToClipboardAsync()
        {
            if (string.IsNullOrEmpty(SelectedFilePath) || _originalFileSize == 0)
                return;

            try
            {
                string fullBase64String = await _base64Service.GetFullBase64StringAsync(SelectedFilePath);
                Clipboard.SetText(fullBase64String);
                UpdateStatusMessage("Base64 string copied to clipboard successfully.");
                
                // Provide visual feedback
                CopyButtonText = "Copied!";
                _ = Task.Delay(1500).ContinueWith(_ =>
                {
                    CopyButtonText = "Copy to Clipboard";
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                UpdateStatusMessage($"Error copying to clipboard: {ex.Message}", isError: true);
            }
        }

        [RelayCommand]
        private async Task SaveToFileAsync()
        {
            if (string.IsNullOrEmpty(SelectedFilePath) || _originalFileSize == 0)
                return;

            var saveFilePath = _fileService.SaveFileDialog(
                "Save Base64 String to File",
                "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                "base64_output.txt");

            if (!string.IsNullOrEmpty(saveFilePath))
            {
                try
                {
                    string fullBase64String = await _base64Service.GetFullBase64StringAsync(SelectedFilePath);
                    await _fileService.SaveTextToFileAsync(saveFilePath, fullBase64String);
                    UpdateStatusMessage("Base64 string saved to file successfully.");
                    
                    // Provide visual feedback
                    SaveToFileButtonText = "Saved!";
                    _ = Task.Delay(1500).ContinueWith(_ =>
                    {
                        SaveToFileButtonText = "Save to File";
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    UpdateStatusMessage($"Error saving to file: {ex.Message}", isError: true);
                }
            }
        }

        [RelayCommand]
        private void SelectBase64File()
        {
            var filePath = _fileService.OpenFileDialog(
                "Select a Base64 file to decode",
                "Text files (*.txt)|*.txt|All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(filePath))
            {
                CleanupDecodedTemporaryFile();
                CleanupPastedBase64TemporaryFile();

                SelectedBase64FilePath = filePath;
                Base64FileNameDisplay = $"Selected file: {Path.GetFileName(filePath)}";
                IsDecodeButtonEnabled = true;
                IsSaveDecodedButtonEnabled = false;
                IsDecodeResultVisible = false;
                ShowcasedButton = "Decode";

                // Clear paste area when file is selected
                Base64PastePreview = string.Empty;
            }
            else
            {
                SelectedBase64FilePath = null;
                Base64FileNameDisplay = "No file selected.";
                IsDecodeButtonEnabled = false;
                IsSaveDecodedButtonEnabled = false;
                IsDecodeResultVisible = false;
                ShowcasedButton = "SelectBase64File";
            }
        }

        [RelayCommand]
        private async Task PasteBase64Async()
        {
            try
            {
                string clipboardContent = Clipboard.GetText();

                if (string.IsNullOrWhiteSpace(clipboardContent))
                {
                    UpdateStatusMessage("Clipboard is empty. Please copy a Base64 string first.", isError: true);
                    return;
                }

                if (!_base64Service.IsValidBase64(clipboardContent))
                {
                    UpdateStatusMessage("Clipboard content does not appear to be valid Base64.", isError: true);
                    return;
                }

                // Create a temporary file with the pasted Base64 content
                CleanupPastedBase64TemporaryFile();
                _pastedBase64TemporaryFilePath = Path.Combine(Path.GetTempPath(), $"pasted_base64_{Guid.NewGuid()}.txt");
                await File.WriteAllTextAsync(_pastedBase64TemporaryFilePath, clipboardContent);

                // Display truncated preview
                string preview = clipboardContent.Length > 100
                    ? clipboardContent.Substring(0, 100) + "... [Truncated]"
                    : clipboardContent;
                Base64PastePreview = preview;

                // Clear file selection to avoid confusion
                SelectedBase64FilePath = null;
                Base64FileNameDisplay = "No file selected.";

                // Enable decode button
                IsDecodeButtonEnabled = true;
                IsSaveDecodedButtonEnabled = false;
                IsDecodeResultVisible = false;
                ShowcasedButton = "Decode";

                UpdateStatusMessage("Base64 string pasted from clipboard successfully.");
                
                // Provide visual feedback
                PasteBase64ButtonText = "Pasted!";
                _ = Task.Delay(1500).ContinueWith(_ =>
                {
                    PasteBase64ButtonText = "Paste from Clipboard";
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                UpdateStatusMessage($"Error pasting from clipboard: {ex.Message}", isError: true);
            }
        }

        [RelayCommand]
        private async Task DecodeBase64Async()
        {
            // Determine which source to use: file or pasted content
            string? sourceFilePath = null;

            if (!string.IsNullOrEmpty(_pastedBase64TemporaryFilePath) && _fileService.FileExists(_pastedBase64TemporaryFilePath))
            {
                sourceFilePath = _pastedBase64TemporaryFilePath;
            }
            else if (!string.IsNullOrEmpty(SelectedBase64FilePath) && _fileService.FileExists(SelectedBase64FilePath))
            {
                sourceFilePath = SelectedBase64FilePath;
            }
            else
            {
                UpdateStatusMessage("Please select a file or paste a Base64 string and try again.", isError: true);
                return;
            }

            UpdateStatusMessage("Decoding from Base64...");
            IsProcessing = true;
            IsDecodeButtonEnabled = false;
            IsSaveDecodedButtonEnabled = false;
            IsDecodeResultVisible = false;
            ShowcasedButton = null;

            try
            {
                var result = await _base64Service.DecodeBase64FileAsync(sourceFilePath);

                if (result.IsSuccess)
                {
                    _decodedTemporaryFilePath = result.TemporaryFilePath;
                    DecodeResultInfo = $"Base64 input size: {_base64Service.FormatFileSize(result.OriginalSize)}\n" +
                                      $"Decoded binary size: {_base64Service.FormatFileSize(result.ConvertedSize)}";
                    IsDecodeResultVisible = true;
                    IsSaveDecodedButtonEnabled = true;
                    UpdateStatusMessage($"Decoding complete. Decoded size: {_base64Service.FormatFileSize(result.ConvertedSize)}.");
                    ShowcasedButton = "SaveDecoded";
                }
                else
                {
                    UpdateStatusMessage(result.ErrorMessage ?? "Unknown error occurred.", isError: true);
                    _decodedTemporaryFilePath = null;
                    IsSaveDecodedButtonEnabled = false;
                    IsDecodeResultVisible = false;
                    ShowcasedButton = "Decode";
                }
            }
            catch (Exception ex)
            {
                UpdateStatusMessage($"Error: {ex.Message}", isError: true);
            }
            finally
            {
                IsProcessing = false;
                IsDecodeButtonEnabled = true;
            }
        }

        [RelayCommand]
        private async Task SaveDecodedFileAsync()
        {
            if (string.IsNullOrEmpty(_decodedTemporaryFilePath))
            {
                UpdateStatusMessage("No decoded content available. Please decode a file first.", isError: true);
                return;
            }

            var saveFilePath = _fileService.SaveFileDialog(
                "Save Decoded Binary Content to File",
                "All files (*.*)|*.*",
                "decoded_output");

            if (!string.IsNullOrEmpty(saveFilePath))
            {
                try
                {
                    await _fileService.CopyFileAsync(_decodedTemporaryFilePath, saveFilePath);
                    UpdateStatusMessage("Decoded content saved to file successfully.");
                    CleanupDecodedTemporaryFile();
                    
                    // Provide visual feedback
                    SaveDecodedButtonText = "Saved!";
                    _ = Task.Delay(1500).ContinueWith(_ =>
                    {
                        SaveDecodedButtonText = "Save to File";
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                catch (Exception ex)
                {
                    UpdateStatusMessage($"Error saving to file: {ex.Message}", isError: true);
                }
            }
        }

        [RelayCommand]
        private void ToggleStatusPopup()
        {
            if (CanExpandStatusMessage)
            {
                IsStatusPopupOpen = !IsStatusPopupOpen;
            }
        }

        [RelayCommand]
        private void CloseStatusPopup()
        {
            IsStatusPopupOpen = false;
        }

        #endregion

        #region Mode Change

        partial void OnSelectedTabIndexChanged(int value)
        {
            CurrentMode = value == 1 ? AppMode.Decode : AppMode.Encode;
            AccentBrush = CurrentMode == AppMode.Encode ? EncodeAccentBrush : DecodeAccentBrush;
            UpdateStatusMessage($"Ready in {CurrentMode} mode.");
            ShowcasedButton = CurrentMode == AppMode.Encode ? "SelectFile" : "SelectBase64File";
        }

        #endregion

        #region Helper Methods

        private void UpdateStatusMessage(string message, bool isError = false)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string messageWithTimestamp = $"[{timestamp}] {message}";

            FullStatusMessage = messageWithTimestamp;

            const int maxStatusBarLength = 100;
            string displayMessage = messageWithTimestamp;

            if (messageWithTimestamp.Length > maxStatusBarLength)
            {
                displayMessage = messageWithTimestamp.Substring(0, maxStatusBarLength) + "... (click to expand)";
                CanExpandStatusMessage = true;
            }
            else
            {
                CanExpandStatusMessage = false;
            }

            StatusMessage = displayMessage;
            StatusForeground = isError ? ErrorBrush : NeutralBrush;

            // Auto-focus status bar when error occurs
            if (isError)
            {
                ShouldFocusStatusBar = false; // Reset
                ShouldFocusStatusBar = true;  // Trigger focus
            }
        }

        private void CleanupDecodedTemporaryFile()
        {
            if (!string.IsNullOrEmpty(_decodedTemporaryFilePath))
            {
                _fileService.DeleteFile(_decodedTemporaryFilePath);
                _decodedTemporaryFilePath = null;
            }
        }

        private void CleanupPastedBase64TemporaryFile()
        {
            if (!string.IsNullOrEmpty(_pastedBase64TemporaryFilePath))
            {
                _fileService.DeleteFile(_pastedBase64TemporaryFilePath);
                _pastedBase64TemporaryFilePath = null;
            }
        }

        public void Dispose()
        {
            CleanupDecodedTemporaryFile();
            CleanupPastedBase64TemporaryFile();
        }

        #endregion
    }
}
