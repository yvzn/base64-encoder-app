using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Animation;

namespace Base64Utils
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _selectedFilePath;
        private string? _fullBase64String;
        private string? _fullStatusMessage;
        private const int MaxDisplayLength = 10000;

        public MainWindow()
        {
            InitializeComponent();
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
                    ExpandedMessageText.Foreground = StatusBarText.Foreground;
                    StatusMessagePopup.IsOpen = true;
                }
            }
        }

        private void ClosePopupButton_Click(object sender, RoutedEventArgs e)
        {
            StatusMessagePopup.IsOpen = false;
        }

        private void ShowStatusMessage(string message, bool isError = false)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string messageWithTimestamp = $"[{timestamp}] {message}";
            
            _fullStatusMessage = messageWithTimestamp;
            
            const int maxStatusBarLength = 100;
            if (messageWithTimestamp.Length > maxStatusBarLength)
            {
                StatusBarText.Text = messageWithTimestamp.Substring(0, maxStatusBarLength) + "... (click to expand)";
            }
            else
            {
                StatusBarText.Text = messageWithTimestamp;
            }
            
            StatusBarText.Foreground = isError ? 
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 0, 0)) : 
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(51, 51, 51));
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
                _fullBase64String = null;
                CopyButton.IsEnabled = false;
                SaveToFileButton.IsEnabled = false;
            }
            else
            {
                _selectedFilePath = null;
                FileNameTextBlock.Text = "No file selected.";
                ConvertButton.IsEnabled = false;
                ResultTextBox.Text = string.Empty;
                _fullBase64String = null;
                CopyButton.IsEnabled = false;
                SaveToFileButton.IsEnabled = false;
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

            var spinnerAnimation = (Storyboard)this.Resources["SpinnerAnimation"];
            spinnerAnimation.Begin();
            LoadingSpinner.Visibility = Visibility.Visible;
            ConvertButton.IsEnabled = false;
            SelectFileButton.IsEnabled = false;
            CopyButton.IsEnabled = false;
            SaveToFileButton.IsEnabled = false;

            try
            {
                byte[] fileBytes = await File.ReadAllBytesAsync(_selectedFilePath);
                string base64String = Convert.ToBase64String(fileBytes);

                _fullBase64String = base64String;

                if (base64String.Length > MaxDisplayLength)
                {
                    string truncated = base64String.Substring(0, MaxDisplayLength);
                    ResultTextBox.Text = $"{truncated}\n\n[Truncated - Full length: {base64String.Length:N0} characters. Click 'Copy to Clipboard' to copy the complete Base64 string.]";
                }
                else
                {
                    ResultTextBox.Text = base64String;
                }

                CopyButton.IsEnabled = true;
                SaveToFileButton.IsEnabled = true;
                ShowStatusMessage($"Conversion complete. Generated {base64String.Length:N0} characters.");
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Error converting file: {ex.Message}. Please try again or select another file.", isError: true);
                _fullBase64String = null;
                CopyButton.IsEnabled = false;
                SaveToFileButton.IsEnabled = false;
            }
            finally
            {
                spinnerAnimation.Stop();
                LoadingSpinner.Visibility = Visibility.Collapsed;
                ConvertButton.IsEnabled = true;
                SelectFileButton.IsEnabled = true;
            }
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_fullBase64String))
            {
                try
                {
                    Clipboard.SetText(_fullBase64String);

                    string originalText = CopyButton.Content.ToString() ?? "Copy to Clipboard";
                    CopyButton.Content = "Copied!";
                    CopyButton.IsEnabled = false;

                    await Task.Delay(1500);

                    CopyButton.Content = originalText;
                    CopyButton.IsEnabled = true;
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
            if (!string.IsNullOrEmpty(_fullBase64String))
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
                        await File.WriteAllTextAsync(saveFileDialog.FileName, _fullBase64String);

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
    }
}