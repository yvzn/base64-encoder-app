using System.IO;
using System.Text;

namespace Base64Utils.Services
{
    public class FileTypeDetectionService : IFileTypeDetectionService
    {
        private class FileSignature
        {
            public byte[] Signature { get; set; } = Array.Empty<byte>();
            public int Offset { get; set; }
            public string FileType { get; set; } = string.Empty;
            public string Extension { get; set; } = string.Empty;
        }

        private static readonly List<FileSignature> _fileSignatures = new()
        {
            // Images - PNG
            new FileSignature { Signature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, Offset = 0, FileType = "PNG Image", Extension = "png" },
            
            // Images - JPEG
            new FileSignature { Signature = new byte[] { 0xFF, 0xD8, 0xFF, 0xDB }, Offset = 0, FileType = "JPEG Image", Extension = "jpg" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, Offset = 0, FileType = "JPEG Image", Extension = "jpg" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, Offset = 0, FileType = "JPEG Image", Extension = "jpg" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, Offset = 0, FileType = "JPEG Image", Extension = "jpg" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, Offset = 0, FileType = "JPEG Image", Extension = "jpg" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }, Offset = 0, FileType = "JPEG Image", Extension = "jpg" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xD8, 0xFF, 0xEE }, Offset = 0, FileType = "JPEG Image", Extension = "jpg" },
            
            // Images - GIF
            new FileSignature { Signature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, Offset = 0, FileType = "GIF Image", Extension = "gif" }, // GIF87a
            new FileSignature { Signature = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, Offset = 0, FileType = "GIF Image", Extension = "gif" }, // GIF89a
            
            // Images - WebP
            new FileSignature { Signature = new byte[] { 0x52, 0x49, 0x46, 0x46 }, Offset = 0, FileType = "WebP Image", Extension = "webp" }, // RIFF (needs additional check at offset 8)
            
            // Images - BMP
            new FileSignature { Signature = new byte[] { 0x42, 0x4D }, Offset = 0, FileType = "BMP Image", Extension = "bmp" },
            
            // Images - TIFF
            new FileSignature { Signature = new byte[] { 0x49, 0x49, 0x2A, 0x00 }, Offset = 0, FileType = "TIFF Image", Extension = "tif" }, // Little-endian
            new FileSignature { Signature = new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, Offset = 0, FileType = "TIFF Image", Extension = "tif" }, // Big-endian
            
            // Images - ICO
            new FileSignature { Signature = new byte[] { 0x00, 0x00, 0x01, 0x00 }, Offset = 0, FileType = "ICO Image", Extension = "ico" },
            
            // Images - SVG (XML-based, will be detected as text)
            
            // Documents - PDF
            new FileSignature { Signature = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }, Offset = 0, FileType = "PDF Document", Extension = "pdf" },
            
            // Documents - Microsoft Office (Legacy - Office 97-2003)
            new FileSignature { Signature = new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, Offset = 0, FileType = "Microsoft Office Document", Extension = "doc" },
            
            // Documents - Microsoft Office (Modern - Office 2007+)
            new FileSignature { Signature = new byte[] { 0x50, 0x4B, 0x03, 0x04 }, Offset = 0, FileType = "ZIP Archive or Office Document", Extension = "zip" }, // Also ZIP, DOCX, XLSX, PPTX
            new FileSignature { Signature = new byte[] { 0x50, 0x4B, 0x05, 0x06 }, Offset = 0, FileType = "ZIP Archive or Office Document", Extension = "zip" }, // Empty ZIP
            new FileSignature { Signature = new byte[] { 0x50, 0x4B, 0x07, 0x08 }, Offset = 0, FileType = "ZIP Archive or Office Document", Extension = "zip" }, // Spanned ZIP
            
            // Documents - RTF
            new FileSignature { Signature = new byte[] { 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31 }, Offset = 0, FileType = "Rich Text Format", Extension = "rtf" }, // {\rtf1
            
            // Fonts - TTF
            new FileSignature { Signature = new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00 }, Offset = 0, FileType = "TrueType Font", Extension = "ttf" },
            
            // Fonts - OTF
            new FileSignature { Signature = new byte[] { 0x4F, 0x54, 0x54, 0x4F }, Offset = 0, FileType = "OpenType Font", Extension = "otf" }, // OTTO
            
            // Fonts - WOFF
            new FileSignature { Signature = new byte[] { 0x77, 0x4F, 0x46, 0x46 }, Offset = 0, FileType = "Web Open Font Format", Extension = "woff" }, // wOFF
            
            // Fonts - WOFF2
            new FileSignature { Signature = new byte[] { 0x77, 0x4F, 0x46, 0x32 }, Offset = 0, FileType = "Web Open Font Format 2", Extension = "woff2" }, // wOF2
            
            // Fonts - EOT
            new FileSignature { Signature = new byte[] { 0x4C, 0x50 }, Offset = 34, FileType = "Embedded OpenType Font", Extension = "eot" },
            
            // Videos - MP4
            new FileSignature { Signature = new byte[] { 0x66, 0x74, 0x79, 0x70, 0x69, 0x73, 0x6F, 0x6D }, Offset = 4, FileType = "MP4 Video", Extension = "mp4" }, // ftypisom
            new FileSignature { Signature = new byte[] { 0x66, 0x74, 0x79, 0x70, 0x4D, 0x53, 0x4E, 0x56 }, Offset = 4, FileType = "MP4 Video", Extension = "mp4" }, // ftypMSNV
            new FileSignature { Signature = new byte[] { 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32 }, Offset = 4, FileType = "MP4 Video", Extension = "mp4" }, // ftypmp42
            
            // Videos - AVI
            new FileSignature { Signature = new byte[] { 0x52, 0x49, 0x46, 0x46 }, Offset = 0, FileType = "AVI Video", Extension = "avi" }, // RIFF (needs additional check at offset 8 for "AVI ")
            
            // Videos - WebM
            new FileSignature { Signature = new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, Offset = 0, FileType = "WebM Video", Extension = "webm" },
            
            // Videos - MKV
            new FileSignature { Signature = new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, Offset = 0, FileType = "Matroska Video", Extension = "mkv" },
            
            // Videos - FLV
            new FileSignature { Signature = new byte[] { 0x46, 0x4C, 0x56, 0x01 }, Offset = 0, FileType = "Flash Video", Extension = "flv" },
            
            // Videos - MOV
            new FileSignature { Signature = new byte[] { 0x66, 0x74, 0x79, 0x70, 0x71, 0x74, 0x20, 0x20 }, Offset = 4, FileType = "QuickTime Video", Extension = "mov" }, // ftypqt
            
            // Videos - WMV
            new FileSignature { Signature = new byte[] { 0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11, 0xA6, 0xD9 }, Offset = 0, FileType = "Windows Media Video", Extension = "wmv" },
            
            // Videos - MPEG
            new FileSignature { Signature = new byte[] { 0x00, 0x00, 0x01, 0xBA }, Offset = 0, FileType = "MPEG Video", Extension = "mpg" },
            new FileSignature { Signature = new byte[] { 0x00, 0x00, 0x01, 0xB3 }, Offset = 0, FileType = "MPEG Video", Extension = "mpg" },
            
            // Audio - MP3
            new FileSignature { Signature = new byte[] { 0xFF, 0xFB }, Offset = 0, FileType = "MP3 Audio", Extension = "mp3" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xF3 }, Offset = 0, FileType = "MP3 Audio", Extension = "mp3" },
            new FileSignature { Signature = new byte[] { 0xFF, 0xF2 }, Offset = 0, FileType = "MP3 Audio", Extension = "mp3" },
            new FileSignature { Signature = new byte[] { 0x49, 0x44, 0x33 }, Offset = 0, FileType = "MP3 Audio", Extension = "mp3" }, // ID3
            
            // Audio - WAV
            new FileSignature { Signature = new byte[] { 0x52, 0x49, 0x46, 0x46 }, Offset = 0, FileType = "WAV Audio", Extension = "wav" }, // RIFF (needs additional check at offset 8 for "WAVE")
            
            // Audio - OGG
            new FileSignature { Signature = new byte[] { 0x4F, 0x67, 0x67, 0x53 }, Offset = 0, FileType = "OGG Audio", Extension = "ogg" },
            
            // Audio - FLAC
            new FileSignature { Signature = new byte[] { 0x66, 0x4C, 0x61, 0x43 }, Offset = 0, FileType = "FLAC Audio", Extension = "flac" },
            
            // Archives - 7Z
            new FileSignature { Signature = new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C }, Offset = 0, FileType = "7-Zip Archive", Extension = "7z" },
            
            // Archives - RAR
            new FileSignature { Signature = new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00 }, Offset = 0, FileType = "RAR Archive", Extension = "rar" }, // RAR 1.5+
            new FileSignature { Signature = new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 }, Offset = 0, FileType = "RAR Archive", Extension = "rar" }, // RAR 5.0+
            
            // Archives - GZIP
            new FileSignature { Signature = new byte[] { 0x1F, 0x8B }, Offset = 0, FileType = "GZIP Archive", Extension = "gz" },
            
            // Archives - TAR
            new FileSignature { Signature = new byte[] { 0x75, 0x73, 0x74, 0x61, 0x72 }, Offset = 257, FileType = "TAR Archive", Extension = "tar" }, // ustar
            
            // Executables - EXE
            new FileSignature { Signature = new byte[] { 0x4D, 0x5A }, Offset = 0, FileType = "Windows Executable", Extension = "exe" },
            
            // Executables - ELF
            new FileSignature { Signature = new byte[] { 0x7F, 0x45, 0x4C, 0x46 }, Offset = 0, FileType = "ELF Executable", Extension = "elf" },
            
            // Other - XML
            new FileSignature { Signature = new byte[] { 0x3C, 0x3F, 0x78, 0x6D, 0x6C, 0x20 }, Offset = 0, FileType = "XML Document", Extension = "xml" }, // <?xml
        };

        public async Task<(string FileType, string Extension)> DetectFileTypeAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return ("Unknown", "bin");
                }

                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                
                // Read up to 512 bytes for signature checking
                byte[] buffer = new byte[512];
                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    return ("Empty File", "bin");
                }

                // Check binary file signatures
                foreach (var signature in _fileSignatures)
                {
                    if (signature.Offset + signature.Signature.Length <= bytesRead)
                    {
                        bool match = true;
                        for (int i = 0; i < signature.Signature.Length; i++)
                        {
                            if (buffer[signature.Offset + i] != signature.Signature[i])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (match)
                        {
                            // Special handling for RIFF files (WebP, AVI, WAV)
                            if (signature.Extension == "webp" && bytesRead >= 12)
                            {
                                // Check for "WEBP" at offset 8
                                if (buffer[8] == 0x57 && buffer[9] == 0x45 && buffer[10] == 0x42 && buffer[11] == 0x50)
                                {
                                    return ("WebP Image", "webp");
                                }
                            }
                            else if (signature.FileType == "AVI Video" && bytesRead >= 12)
                            {
                                // Check for "AVI " at offset 8
                                if (buffer[8] == 0x41 && buffer[9] == 0x56 && buffer[10] == 0x49 && buffer[11] == 0x20)
                                {
                                    return ("AVI Video", "avi");
                                }
                            }
                            else if (signature.FileType == "WAV Audio" && bytesRead >= 12)
                            {
                                // Check for "WAVE" at offset 8
                                if (buffer[8] == 0x57 && buffer[9] == 0x41 && buffer[10] == 0x56 && buffer[11] == 0x45)
                                {
                                    return ("WAV Audio", "wav");
                                }
                            }
                            else if (signature.Extension == "zip" && bytesRead >= 30)
                            {
                                // Try to determine if it's a modern Office document
                                // Check for common Office file structures
                                string bufferString = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                
                                if (bufferString.Contains("word/"))
                                {
                                    return ("Microsoft Word Document", "docx");
                                }
                                else if (bufferString.Contains("xl/"))
                                {
                                    return ("Microsoft Excel Spreadsheet", "xlsx");
                                }
                                else if (bufferString.Contains("ppt/"))
                                {
                                    return ("Microsoft PowerPoint Presentation", "pptx");
                                }
                                else
                                {
                                    return ("ZIP Archive", "zip");
                                }
                            }
                            else
                            {
                                return (signature.FileType, signature.Extension);
                            }
                        }
                    }
                }

                // Check for text files with BOM (Byte Order Mark)
                if (bytesRead >= 3)
                {
                    // UTF-8 BOM
                    if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                    {
                        return ("UTF-8 Text File", "txt");
                    }
                }

                if (bytesRead >= 2)
                {
                    // UTF-16 LE BOM
                    if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                    {
                        return ("UTF-16 LE Text File", "txt");
                    }
                    // UTF-16 BE BOM
                    if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                    {
                        return ("UTF-16 BE Text File", "txt");
                    }
                }

                if (bytesRead >= 4)
                {
                    // UTF-32 LE BOM
                    if (buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00)
                    {
                        return ("UTF-32 LE Text File", "txt");
                    }
                    // UTF-32 BE BOM
                    if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0xFE && buffer[3] == 0xFF)
                    {
                        return ("UTF-32 BE Text File", "txt");
                    }
                }

                // Check if it's plain text (ASCII or UTF-8 without BOM)
                if (IsLikelyTextFile(buffer, bytesRead))
                {
                    // Try to detect specific text file types
                    string textSample = Encoding.UTF8.GetString(buffer, 0, Math.Min(bytesRead, 256));
                    
                    if (textSample.TrimStart().StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
                        textSample.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                    {
                        return ("HTML Document", "html");
                    }
                    else if (textSample.TrimStart().StartsWith("{") || textSample.TrimStart().StartsWith("["))
                    {
                        // Might be JSON
                        return ("JSON File", "json");
                    }
                    else if (textSample.Contains("<?xml"))
                    {
                        return ("XML Document", "xml");
                    }
                    else if (textSample.TrimStart().StartsWith("<svg"))
                    {
                        return ("SVG Image", "svg");
                    }
                    
                    return ("Plain Text File", "txt");
                }

                // Unknown binary file
                return ("Unknown", "bin");
            }
            catch
            {
                return ("Unknown", "bin");
            }
        }

        private bool IsLikelyTextFile(byte[] buffer, int length)
        {
            // Check if the file contains mostly printable characters
            int printableCount = 0;
            int nullCount = 0;
            int controlCount = 0;

            for (int i = 0; i < length; i++)
            {
                byte b = buffer[i];

                if (b == 0)
                {
                    nullCount++;
                }
                else if (b == 0x09 || b == 0x0A || b == 0x0D) // Tab, LF, CR
                {
                    printableCount++;
                }
                else if (b < 0x20 || b == 0x7F) // Control characters
                {
                    controlCount++;
                }
                else if (b < 0x7F) // ASCII printable
                {
                    printableCount++;
                }
                else if (b >= 0x80) // Could be UTF-8 multi-byte
                {
                    // Accept as potentially valid UTF-8
                    printableCount++;
                }
            }

            // If there are null bytes, it's likely binary
            if (nullCount > 0)
            {
                return false;
            }

            // If most characters are printable, consider it text
            double printableRatio = (double)printableCount / length;
            return printableRatio > 0.85; // At least 85% printable
        }
    }
}
