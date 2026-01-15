using System.Windows;
using Base64Utils.Services;
using Base64Utils.ViewModels;

namespace Base64Utils
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize services
            var fileTypeDetectionService = new FileTypeDetectionService();
            var base64Service = new Base64Service(fileTypeDetectionService);
            var fileService = new FileService();

            // Initialize ViewModel
            _viewModel = new MainViewModel(base64Service, fileService);

            // Set DataContext for data binding
            DataContext = _viewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel?.Dispose();
        }
    }
}
