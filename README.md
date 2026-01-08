# Base64Utils

A Windows desktop application for converting files to Base64 encoding.

## What It Does

- Select any file and convert it to a Base64 string
- Copy the Base64 output to clipboard
- Save the Base64 output to a text file

## Tech Stack

- **Language**: C# (.NET 10)
- **Framework**: WPF (Windows Presentation Foundation)
- **Target Platform**: Windows (x64)

## Build and Run Locally

Requirements:
- .NET 10 SDK

```bash
# Clone or download the repository
git clone <repository-url>
cd <repository-directory>

# Build the project
dotnet build

# Run the application
dotnet run
```

## Publish

### Self-Contained (includes all required dependencies)
```bash
dotnet publish -c Release --os win --arch x64 --self-contained
```

### Framework-Dependent (requires .NET 10 installed on user's computer)
```bash
dotnet publish -c Release --os win --arch x64 --no-self-contained
```

Output will be in `bin/Release/net10.0-windows/win-x64/publish/`

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
