# Requirements

## Description

better_windows_terminal is a sleek, minimal Windows terminal wrapper built with C# and WPF. Featuring a pure black UI with animated glowing purple particles, Chrome-style tabs, async process management for PowerShell and CMD, custom dark scrollbars, and single-file packaging. Designed to replace the default Windows terminal with a cleaner, more immersive experience.

## Build Command

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
```

Output: `bin/Release/net8.0-windows/win-x64/publish/better_windows_terminal.exe`

## System Requirements

- Windows 10 or Windows 11 (x64)
- No .NET runtime installation needed (self-contained)
- No additional dependencies
