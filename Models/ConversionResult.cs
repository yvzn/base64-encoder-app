namespace Base64Utils.Models
{
    public class ConversionResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public long OriginalSize { get; set; }
        public long ConvertedSize { get; set; }
        public string? Preview { get; set; }
        public string? TemporaryFilePath { get; set; }
    }
}
