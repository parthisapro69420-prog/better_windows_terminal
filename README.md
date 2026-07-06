# better_windows_terminal

A custom Windows terminal wrapper built with C# and WPF (.NET 8.0).

Fully black, minimal UI with animated glowing purple particles and Chrome-style tabs.

NOTE: THIS PROJECT HAS BEEN ABANDON, THERE IS NO 0.9.0-ALPHA OR ANY FIXES.

## Features

- Borderless, sleek dark window
- Chrome-style tab system with PowerShell and CMD support
- Async process management per tab (stdin/stdout/stderr)
- Animated glowing particle system on the edges
- Custom dark scrollbars
- `cls` / `clear` support
- Single-file executable build

## Demo



https://github.com/user-attachments/assets/f1b67848-3668-412c-9f1e-3b936dad7fe8



## Build

```bash
dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:SelfContained=true
```

Output: `bin/Release/net8.0-windows/win-x64/publish/better_windows_terminal.exe`

## License

All rights reserved. See [LICENSE](LICENSE) for details.

No forking or copying without permission. Contact **parthisapro69420@gmail.com** to request access or collaborate.
