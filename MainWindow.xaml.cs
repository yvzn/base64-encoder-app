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
        private string? _fullStatusMessage;
        private bool _lastStatusWasError;
        private Mode _currentMode = Mode.Encode;
        private readonly SolidColorBrush _encodeAccent = new SolidColorBrush(Color.FromRgb(0, 120, 212));
        private readonly SolidColorBrush _decodeAccent = new SolidColorBrush(Color.FromRgb(16, 124, 16));
        private const int MaxDisplayLength = 10000;

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
                ResultTextBox.Text = string.Empty;
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
                ResultTextBox.Text = string.Empty;
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

            try
            {
                FileInfo fileInfo = new FileInfo(_selectedFilePath);
                _fileSize = fileInfo.Length;
                
                // Calculate how many bytes we need to read for the display preview
                // Base64 encoding increases size by ~4/3, so we need (MaxDisplayLength * 3 / 4) bytes
                int bytesToRead = (int)Math.Min(_fileSize, (MaxDisplayLength * 3 / 4));
                
                string previewBase64;
                long estimatedBase64Length;
                
                using (FileStream inputFile = new FileStream(_selectedFilePath, FileMode.Open, FileAccess.Read))
                using (CryptoStream base64Stream = new CryptoStream(inputFile, new ToBase64Transform(), CryptoStreamMode.Read))
                {
                    byte[] buffer = new byte[MaxDisplayLength];
                    int bytesRead = await base64Stream.ReadAsync(buffer, 0, MaxDisplayLength);
                    previewBase64 = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
                
                // Calculate estimated full Base64 length
                estimatedBase64Length = ((_fileSize + 2) / 3) * 4;

                if (estimatedBase64Length > MaxDisplayLength)
                {
                    ResultTextBox.Text = $"{previewBase64}\n\n[Truncated - Full length: {estimatedBase64Length:N0} characters. Click 'Copy to Clipboard' to copy the complete Base64 string.]";
                }
                else
                {
                    ResultTextBox.Text = previewBase64;
                }

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

                    await Task.Delay(1500);

                    CopyButton.Content = originalText;
                    // Restore the showcase highlighting
                    ShowcaseButton(CopyButton);
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

                        await Task.Delay(1500);

                        SaveToFileButton.Content = originalText;
                        SaveToFileButton.IsEnabled = true;
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
                ShowStatusMessage("Decode mode selected. Tools coming soon.");
            }
            else
            {
                _currentMode = Mode.Encode;
                ApplyModeAccent();
                ShowStatusMessage("Encode mode selected. Ready.");
                ShowcaseButton(SelectFileButton);
            }
        }
    }
}