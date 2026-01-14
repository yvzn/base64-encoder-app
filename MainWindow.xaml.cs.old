using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Documents;

namespace Base64Utils
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Mode
        {
            Encode,
            Decode
        }

        private string? _selectedFilePath;
        private long _fileSize;
        private string? _selectedBase64FilePath;
        private long _base64FileSize;
        private string? _fullStatusMessage;
        private bool _lastStatusWasError;
        private Mode _currentMode = Mode.Encode;
        private readonly SolidColorBrush _encodeAccent = new SolidColorBrush(Color.FromRgb(64, 162, 227));
        private readonly SolidColorBrush _decodeAccent = new SolidColorBrush(Color.FromRgb(13, 146, 118));
        private const int MaxDisplayLength = 10000;
        private string? _decodedTemporaryFilePath;
        private long _decodedFileSize;
        private string? _pastedBase64TemporaryFilePath;

        public MainWindow()
        {
            InitializeComponent();
            ApplyModeAccent();
            ShowStatusMessage("Ready in Encode mode.");
            ShowcaseButton(SelectFileButton);
        }

        private void StatusBarText_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!string.IsNullOrEmpty(_fullStatusMessage) && _fullStatusMessage != "Ready")
            {
                if (StatusMessagePopup.IsOpen)
                {
                    StatusMessagePopup.IsOpen = false;
                }
                else
                {
                    ExpandedMessageText.Text = _fullStatusMessage;
                    StatusMessagePopup.IsOpen = true;
                }
            }
        }

        private void ClosePopupButton_Click(object sender, RoutedEventArgs e)
        {
            StatusMessagePopup.IsOpen = false;
        }

        private void StatusBarText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Space)
            {
                if (!string.IsNullOrEmpty(_fullStatusMessage) && _fullStatusMessage != "Ready")
                {
                    if (StatusMessagePopup.IsOpen)
                    {
                        StatusMessagePopup.IsOpen = false;
                    }
                    else
                    {
                        ExpandedMessageText.Text = _fullStatusMessage;
                        StatusMessagePopup.IsOpen = true;
                    }
                }
                e.Handled = true;
            }
        }

        private void StatusMessagePopup_Opened(object sender, EventArgs e)
        {
            // Set focus to the close button when popup opens for keyboard accessibility
            ClosePopupButton.Focus();
        }

        private void ClosePopupButton_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                StatusMessagePopup.IsOpen = false;
                // Return focus to the status bar text
                StatusBarText.Focus();
                e.Handled = true;
            }
        }

        private void ShowcaseButton(System.Windows.Controls.Button button)
        {
            // Reset all buttons first
            StopShowcase();

            // Add visual emphasis with styling (color only, no animation)
            button.FontWeight = FontWeights.Bold;
            var accent = GetAccentBrush();
            button.Background = accent;
            button.Foreground = System.Windows.Media.Brushes.White;
            
            // Set keyboard focus to the highlighted button
            if (button.IsEnabled)
            {
                button.Focus();
            }
        }

        private void StopShowcase()
        {
            // Reset all buttons to default appearance
            ResetButtonAppearance(SelectFileButton);
            ResetButtonAppearance(ConvertButton);
            ResetButtonAppearance(CopyButton);
            ResetButtonAppearance(SelectBase64FileButton);
            ResetButtonAppearance(DecodeButton);
            ResetButtonAppearance(SaveDecodedToFileButton);
        }

        private void ResetButtonAppearance(System.Windows.Controls.Button button)
        {
            button.FontWeight = FontWeights.Normal;
            button.ClearValue(System.Windows.Controls.Control.BackgroundProperty);
            button.ClearValue(System.Windows.Controls.Control.ForegroundProperty);
        }

        private void ShowStatusMessage(string message, bool isError = false)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string timestampToken = $"[{timestamp}]";
            string messageWithTimestamp = $"{timestampToken} {message}";
            
            _fullStatusMessage = messageWithTimestamp;
            _lastStatusWasError = isError;
            
            const int maxStatusBarLength = 100;
            StatusBarText.Inlines.Clear();

            string suffix = string.Empty;
            string content = message;
            int baseLen = timestampToken.Length + 1; // timestamp + space
            if (messageWithTimestamp.Length > maxStatusBarLength)
            {
                int allowedContentLen = Math.Max(0, maxStatusBarLength - baseLen);
                content = content.Length > allowedContentLen ? content.Substring(0, allowedContentLen) : content;
                suffix = "... (click to expand)";
            }

            if (isError)
            {
                var errorBrush = new SolidColorBrush(Color.FromRgb(204, 0, 0));
                StatusBarText.Foreground = errorBrush;
                StatusBarText.Inlines.Add(new Run($"{timestampToken} {content}{suffix}"));
            }
            else
            {
                // Reset to neutral color for non-error state
                StatusBarText.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51));

                var accent = GetAccentBrush();
                StatusBarText.Inlines.Add(new Run(timestampToken) { Foreground = accent });
                StatusBarText.Inlines.Add(new Run($" {content}{suffix}"));
            }

            StatusBarAccent.Background = GetAccentBrush();
            
            // Move focus to status bar when an error occurs for accessibility
            if (isError)
            {
                StatusBarText.Focus();
            }
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a file to convert to Base64",
                Filter = "All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFilePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(_selectedFilePath);
                FileNameTextBlock.Text = $"Selected file: {fileName}";
                ConvertButton.IsEnabled = true;
                EncodeResultBlock.Visibility = Visibility.Collapsed;
                _fileSize = 0;
                CopyButton.IsEnabled = false;
                SaveToFileButton.IsEnabled = false;
                
                // Showcase the Convert button as the next action
                ShowcaseButton(ConvertButton);
            }
            else
            {
                _selectedFilePath = null;
                FileNameTextBlock.Text = "No file selected.";
                ConvertButton.IsEnabled = false;
                EncodeResultBlock.Visibility = Visibility.Collapsed;
                _fileSize = 0;
                CopyButton.IsEnabled = false;
                SaveToFileButton.IsEnabled = false;
                
                // Showcase the Select File button again
                ShowcaseButton(SelectFileButton);
            }
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                ShowStatusMessage("Please select a valid file and try again.", isError: true);
                return;
            }

            ShowStatusMessage("Converting file to Base64...");

            // Stop showcasing during conversion
            StopShowcase();
            StatusProgress.Foreground = GetAccentBrush();
            StatusProgress.IsIndeterminate = true;
            StatusProgress.Visibility = Visibility.Visible;
            ConvertButton.IsEnabled = false;
            SelectFileButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            SaveToFileButton.IsEnabled = false;
            EncodeResultBlock.Visibility = Visibility.Collapsed;

            try
            {
                FileInfo fileInfo = new FileInfo(_selectedFilePath);
                _fileSize = fileInfo.Length;
                
                // Generate preview (first ~100 characters of Base64)
                const int previewLength = 100;
                string previewBase64;
                long estimatedBase64Length;
                
                using (FileStream inputFile = new FileStream(_selectedFilePath, FileMode.Open, FileAccess.Read))
                using (CryptoStream base64Stream = new CryptoStream(inputFile, new ToBase64Transform(), CryptoStreamMode.Read))
                {
                    byte[] buffer = new byte[previewLength];
                    int bytesRead = await base64Stream.ReadAsync(buffer, 0, previewLength);
                    previewBase64 = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
                
                // Calculate estimated full Base64 length
                estimatedBase64Length = ((_fileSize + 2) / 3) * 4;

                // Update the result block UI
                string sizeInfo = $"Original file size: {FormatFileSize(_fileSize)}\nBase64 output size: {estimatedBase64Length:N0} characters";
                EncodeResultInfo.Text = sizeInfo;
                
                if (estimatedBase64Length > previewLength)
                {
                    EncodePreviewTextBox.Text = $"{previewBase64}... [Truncated]";
                }
                else
                {
                    EncodePreviewTextBox.Text = previewBase64;
                }
                
                EncodeResultBlock.Visibility = Visibility.Visible;

                CopyButton.IsEnabled = true;
                SaveToFileButton.IsEnabled = true;
                ShowStatusMessage($"Conversion complete. Generated {estimatedBase64Length:N0} characters.");
                
                // Showcase the Copy button as the next action
                ShowcaseButton(CopyButton);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error converting file: {ex.Message}. Please try again or select another file.", isError: true);
                _fileSize = 0;
                CopyButton.IsEnabled = false;
                SaveToFileButton.IsEnabled = false;
                EncodeResultBlock.Visibility = Visibility.Collapsed;
                
                // Showcase Convert button again to retry
                ShowcaseButton(ConvertButton);
            }
            finally
            {
                StatusProgress.IsIndeterminate = false;
                StatusProgress.Visibility = Visibility.Collapsed;
                ConvertButton.IsEnabled = true;
                SelectFileButton.IsEnabled = true;
            }
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedFilePath) && _fileSize > 0)
            {
                try
                {
                    // Convert the entire file to Base64
                    string fullBase64String = await ConvertFileToBase64Async(_selectedFilePath);
                    Clipboard.SetText(fullBase64String);

                    string originalText = CopyButton.Content.ToString() ?? "Copy to Clipboard";
                    var accent = GetAccentBrush();
                    
                    // Temporarily reset button appearance for "Copied!" feedback
                    ResetButtonAppearance(CopyButton);
                    CopyButton.Content = "Copied!";
                    CopyButton.Background = accent;
                    CopyButton.Foreground = System.Windows.Media.Brushes.White;
                    CopyButton.FontWeight = FontWeights.Bold;

                    _ = Task.Delay(1500).ContinueWith(_ => 
                    {
                        Dispatcher.Invoke(() =>
                        {
                            CopyButton.Content = originalText;
                            // Restore the showcase highlighting
                            ShowcaseButton(CopyButton);
                        });
                    });

                    await Task.CompletedTask;
                    ShowStatusMessage("Base64 string copied to clipboard successfully.");
                }
                catch (Exception ex)
                {
                    ShowStatusMessage($"Error copying to clipboard: {ex.Message}. Please try again.", isError: true);
                }
            }
        }

        private async void SaveToFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_selectedFilePath) && _fileSize > 0)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Title = "Save Base64 String to File",
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = "base64_output.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        // Convert the entire file to Base64
                        string fullBase64String = await ConvertFileToBase64Async(_selectedFilePath);
                        await File.WriteAllTextAsync(saveFileDialog.FileName, fullBase64String);

                        string originalText = SaveToFileButton.Content.ToString() ?? "Save to File";
                        SaveToFileButton.Content = "Saved!";
                        SaveToFileButton.IsEnabled = false;

                        _ = Task.Delay(1500).ContinueWith(_ => 
                        {
                            Dispatcher.Invoke(() =>
                            {
                                SaveToFileButton.Content = originalText;
                                SaveToFileButton.IsEnabled = true;
                            });
                        });
                        ShowStatusMessage("Base64 string saved to file successfully.");
                    }
                    catch (Exception ex)
                    {
                        ShowStatusMessage($"Error saving to file: {ex.Message}. Please try again or select another location.", isError: true);
                    }
                }
            }
        }

        private async Task<string> ConvertFileToBase64Async(string filePath)
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

        private SolidColorBrush GetAccentBrush()
        {
            return _currentMode == Mode.Encode ? _encodeAccent : _decodeAccent;
        }

        private void ApplyModeAccent()
        {
            var accent = GetAccentBrush();
            StatusBarAccent.Background = accent;
            if (StatusProgress != null)
            {
                StatusProgress.Foreground = accent;
            }
        }

        private void ModeTabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ModeTabControl.SelectedItem == DecodeTab)
            {
                _currentMode = Mode.Decode;
                StopShowcase();
                ApplyModeAccent();
                ShowStatusMessage("Ready in Decode mode.");
                ShowcaseButton(SelectBase64FileButton);
            }
            else
            {
                _currentMode = Mode.Encode;
                ApplyModeAccent();
                ShowStatusMessage("Ready in Encode mode.");
                ShowcaseButton(SelectFileButton);
            }
        }

        private void SelectBase64FileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a Base64 file to decode",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Clean up any existing temporary file before processing a new one
                CleanupDecodedTemporaryFile();
                CleanupPastedBase64TemporaryFile();
                
                _selectedBase64FilePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(_selectedBase64FilePath);
                Base64FileNameTextBlock.Text = $"Selected file: {fileName}";
                DecodeButton.IsEnabled = true;
                _base64FileSize = 0;
                SaveDecodedToFileButton.IsEnabled = false;
                DecodeResultBlock.Visibility = Visibility.Collapsed;
                
                // Clear paste area when file is selected
                Base64PasteTextBox.Text = string.Empty;
                CleanupPastedBase64TemporaryFile();
                
                // Showcase the Decode button as the next action
                ShowcaseButton(DecodeButton);
            }
            else
            {
                _selectedBase64FilePath = null;
                Base64FileNameTextBlock.Text = "No file selected.";
                DecodeButton.IsEnabled = false;
                _base64FileSize = 0;
                SaveDecodedToFileButton.IsEnabled = false;
                DecodeResultBlock.Visibility = Visibility.Collapsed;
                
                // Showcase the Select File button again
                ShowcaseButton(SelectBase64FileButton);
            }
        }

        private void EncodeTab_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle Ctrl+C to copy the encoded Base64 string
            if (e.Key == System.Windows.Input.Key.C && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                // Only proceed if there's a file selected and conversion has been done
                if (!string.IsNullOrEmpty(_selectedFilePath) && _fileSize > 0 && CopyButton.IsEnabled)
                {
                    // Trigger the copy action
                    CopyButton_Click(CopyButton, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }

        private void DecodeTab_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle Ctrl+V to paste from clipboard
            if (e.Key == System.Windows.Input.Key.V && (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
            {
                // Trigger the paste action
                PasteBase64Button_Click(PasteBase64Button, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        private async void PasteBase64Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the clipboard content
                string clipboardContent = Clipboard.GetText();
                
                if (string.IsNullOrWhiteSpace(clipboardContent))
                {
                    ShowStatusMessage("Clipboard is empty. Please copy a Base64 string first.", isError: true);
                    return;
                }

                // Validate that it looks like Base64 (basic validation)
                if (!IsValidBase64(clipboardContent))
                {
                    ShowStatusMessage("Clipboard content does not appear to be valid Base64. Please check and try again.", isError: true);
                    return;
                }

                // Create a temporary file with the pasted Base64 content
                CleanupPastedBase64TemporaryFile();
                _pastedBase64TemporaryFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"pasted_base64_{Guid.NewGuid()}.txt");
                File.WriteAllText(_pastedBase64TemporaryFilePath, clipboardContent);
                
                // Display truncated preview (first 100 characters)
                string preview = clipboardContent.Length > 100 
                    ? clipboardContent.Substring(0, 100) + "... [Truncated]"
                    : clipboardContent;
                Base64PasteTextBox.Text = preview;
                
                // Clear file selection to avoid confusion
                _selectedBase64FilePath = null;
                Base64FileNameTextBlock.Text = "No file selected.";
                
                // Enable decode button
                DecodeButton.IsEnabled = true;
                SaveDecodedToFileButton.IsEnabled = false;
                DecodeResultBlock.Visibility = Visibility.Collapsed;
                
                // Provide visual feedback on the button
                string originalText = PasteBase64Button.Content.ToString() ?? "Paste from Clipboard";
                var accent = GetAccentBrush();
                
                // Temporarily reset button appearance for "Pasted!" feedback
                ResetButtonAppearance(PasteBase64Button);
                PasteBase64Button.Content = "Pasted!";
                PasteBase64Button.FontWeight = FontWeights.Bold;

                _ = Task.Delay(1500).ContinueWith(_ => 
                {
                    Dispatcher.Invoke(() =>
                    {
                        PasteBase64Button.Content = originalText;
                        ResetButtonAppearance(PasteBase64Button);
                    });
                });
                
                ShowStatusMessage("Base64 string pasted from clipboard successfully.");
                
                // Showcase the Decode button as the next action
                ShowcaseButton(DecodeButton);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error pasting from clipboard: {ex.Message}", isError: true);
            }
        }

        private bool IsValidBase64(string content)
        {
            // Remove whitespace and newlines
            string cleaned = System.Text.RegularExpressions.Regex.Replace(content, @"\s+", "");
            
            // Base64 should only contain A-Z, a-z, 0-9, +, /, and = (padding)
            if (!System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^[A-Za-z0-9+/]*={0,2}$"))
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

        private async void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            // Determine which source to use: file or pasted content
            string? sourceFilePath = null;

            if (!string.IsNullOrEmpty(_pastedBase64TemporaryFilePath) && File.Exists(_pastedBase64TemporaryFilePath))
            {
                sourceFilePath = _pastedBase64TemporaryFilePath;
            }
            else if (!string.IsNullOrEmpty(_selectedBase64FilePath) && File.Exists(_selectedBase64FilePath))
            {
                sourceFilePath = _selectedBase64FilePath;
            }
            else
            {
                ShowStatusMessage("Please select a file or paste a Base64 string and try again.", isError: true);
                return;
            }

            ShowStatusMessage("Decoding from Base64...");

            // Stop showcasing during decoding
            StopShowcase();
            StatusProgress.Foreground = GetAccentBrush();
            StatusProgress.IsIndeterminate = true;
            StatusProgress.Visibility = Visibility.Visible;
            DecodeButton.IsEnabled = false;
            SelectBase64FileButton.IsEnabled = false;
            PasteBase64Button.IsEnabled = false;
            SaveDecodedToFileButton.IsEnabled = false;
            DecodeResultBlock.Visibility = Visibility.Collapsed;

            try
            {
                FileInfo fileInfo = new FileInfo(sourceFilePath);
                _base64FileSize = fileInfo.Length;
                
                // Decode Base64 file to binary
                _decodedTemporaryFilePath = await DecodeBase64FileAsync(sourceFilePath);
                _decodedFileSize = new FileInfo(_decodedTemporaryFilePath).Length;
                
                // Update the result block UI
                string sizeInfo = $"Base64 input size: {FormatFileSize(_base64FileSize)}\nDecoded binary size: {FormatFileSize(_decodedFileSize)}";
                DecodeResultInfo.Text = sizeInfo;
                DecodeResultBlock.Visibility = Visibility.Visible;
                
                SaveDecodedToFileButton.IsEnabled = true;
                ShowStatusMessage($"Decoding complete. Decoded size: {FormatFileSize(_decodedFileSize)}.");
                
                // Showcase the Save button as the next action
                ShowcaseButton(SaveDecodedToFileButton);
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error decoding: {ex.Message}. Please try again.", isError: true);
                _base64FileSize = 0;
                _decodedFileSize = 0;
                _decodedTemporaryFilePath = null;
                SaveDecodedToFileButton.IsEnabled = false;
                DecodeResultBlock.Visibility = Visibility.Collapsed;
                
                // Showcase Decode button again to retry
                ShowcaseButton(DecodeButton);
            }
            finally
            {
                StatusProgress.IsIndeterminate = false;
                StatusProgress.Visibility = Visibility.Collapsed;
                DecodeButton.IsEnabled = true;
                SelectBase64FileButton.IsEnabled = true;
                PasteBase64Button.IsEnabled = true;
            }
        }

        private async void SaveDecodedToFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_decodedTemporaryFilePath) || _decodedFileSize == 0)
            {
                ShowStatusMessage("No decoded content available. Please decode a file first.", isError: true);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save Decoded Binary Content to File",
                Filter = "All files (*.*)|*.*",
                FileName = "decoded_output"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Copy the temporary decoded file to the user's selected destination
                    File.Copy(_decodedTemporaryFilePath, saveFileDialog.FileName, overwrite: true);
                    
                    string originalText = SaveDecodedToFileButton.Content.ToString() ?? "Save to File";
                    SaveDecodedToFileButton.Content = "Saved!";
                    SaveDecodedToFileButton.IsEnabled = false;

                    _ = Task.Delay(1500).ContinueWith(_ => 
                    {
                        Dispatcher.Invoke(() =>
                        {
                            SaveDecodedToFileButton.Content = originalText;
                            SaveDecodedToFileButton.IsEnabled = true;
                        });
                    });
                    ShowStatusMessage("Decoded content saved to file successfully.");
                    
                    // Clean up temporary file after successful save
                    CleanupDecodedTemporaryFile();
                }
                catch (Exception ex)
                {
                    ShowStatusMessage($"Error saving to file: {ex.Message}. Please try again or select another location.", isError: true);
                }
            }
        }

        private async Task<string> DecodeBase64FileAsync(string base64FilePath)
        {
            // Read the Base64 content from the file
            string base64Content = await File.ReadAllTextAsync(base64FilePath);
            
            // Create a temporary file for the decoded content
            string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"decoded_{Guid.NewGuid()}.bin");
            
            // Decode Base64 to binary
            byte[] decodedBytes = Convert.FromBase64String(base64Content);
            
            // Write the decoded binary content to the temporary file
            await File.WriteAllBytesAsync(tempFilePath, decodedBytes);
            
            return tempFilePath;
        }

        private string FormatFileSize(long bytes)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Clean up any temporary files when the application closes
            CleanupDecodedTemporaryFile();
            CleanupPastedBase64TemporaryFile();
        }

        private void CleanupDecodedTemporaryFile()
        {
            if (!string.IsNullOrEmpty(_decodedTemporaryFilePath) && File.Exists(_decodedTemporaryFilePath))
            {
                try
                {
                    File.Delete(_decodedTemporaryFilePath);
                    _decodedTemporaryFilePath = null;
                    _decodedFileSize = 0;
                }
                catch (Exception ex)
                {
                    // Log the error but don't show to user as this is cleanup
                    System.Diagnostics.Debug.WriteLine($"Failed to delete temporary file: {ex.Message}");
                }
            }
        }

        private void CleanupPastedBase64TemporaryFile()
        {
            if (!string.IsNullOrEmpty(_pastedBase64TemporaryFilePath) && File.Exists(_pastedBase64TemporaryFilePath))
            {
                try
                {
                    File.Delete(_pastedBase64TemporaryFilePath);
                    _pastedBase64TemporaryFilePath = null;
                }
                catch (Exception ex)
                {
                    // Log the error but don't show to user as this is cleanup
                    System.Diagnostics.Debug.WriteLine($"Failed to delete pasted base64 temporary file: {ex.Message}");
                }
            }
        }
    }
}