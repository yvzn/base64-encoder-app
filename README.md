# Base64Utils

A Windows desktop application for converting files to Base64 encoding.

## What It Does

- Select any file and convert it to a Base64 string
- Copy the Base64 output to clipboard
- Save the Base64 output to a text file

## Tech Stack

- **Language**: C# (.NET 9)
- **Framework**: WPF (Windows Presentation Foundation)
- **Target Platform**: Windows (x64)

## Build and Run Locally

Requirements:
- .NET 9 SDK

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

### Framework-Dependent (requires .NET 9 installed on user's computer)
```bash
dotnet publish -c Release --os win --arch x64 --no-self-contained
```

Output will be in `bin/Release/net9.0-windows/win-x64/publish/`
