# better_windows_terminal

A custom Windows terminal wrapper built with C# and WPF (.NET 8.0).

Fully black, minimal UI with animated glowing purple particles and Chrome-style tabs.

## Features

- Borderless, sleek dark window
- Chrome-style tab system with PowerShell and CMD support
- Async process management per tab (stdin/stdout/stderr)
- Animated glowing particle system on the edges
- Custom dark scrollbars
- `cls` / `clear` support
- Single-file executable build

## Demo

<!-- Add your video here -->

## Build

```bash
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:SelfContained=true
```

Output: `bin/Release/net8.0-windows/win-x64/publish/better_windows_terminal.exe`

## License

All rights reserved. See [LICENSE](LICENSE) for details.

No forking or copying without permission. Contact **parthisapro69420@gmail.com** to request access or collaborate.
