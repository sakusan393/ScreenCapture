# ScreenCapture

**English** | [日本語](README_JA.md)

A Windows screenshot tool with text annotations, image overlays, and painting tools.

[![Release](https://img.shields.io/github/v/release/sakusan393/ScreenCapture)](https://github.com/sakusan393/ScreenCapture/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/sakusan393/ScreenCapture/total)](https://github.com/sakusan393/ScreenCapture/releases)
[![License](https://img.shields.io/github/license/sakusan393/ScreenCapture)](LICENSE)

## Features

### Screen capture

- Drag to select and capture an area.
- **Ctrl+click** to capture the window under the pointer.
- Supports multiple monitors.
- Press **Ctrl+C** to copy the result to the clipboard.
- Press **Ctrl+S** to save the result as an image. The current implementation encodes it as PNG.

### Text annotations

- Right-click to add text.
- Uses the embedded **Noto Sans JP SemiBold** font; no system font installation is required.
- Choose text and background colors with alpha-aware color pickers.
- Change the size with the [+] and [-] buttons; hold a button for continuous adjustment.
- Double-click to edit the text again.
- Drag to reposition text.
- Text color, background color, and size are retained between launches.

### Image overlays

- Press **Ctrl+V** to add an image from the clipboard.
- Paste JPG, JPEG, or PNG files copied in File Explorer, including multiple files at once.
- Preserves transparent PNG alpha and proportionally scales large images to fit the capture area.
- Drag a corner handle to resize; hold Shift to preserve the aspect ratio.
- Drag the top-center handle to rotate.
- Drag an image to reposition it.
- Toggle 10 px or 20 px borders.
- Change the border color with an alpha-aware color picker.

### Painting

- Press **Alt** to turn paint mode on or off.
- Paint mode is also available from the right-click menu.
- Choose an alpha-aware paint color.
- Select one of four stroke widths: 1, 3, 5, or 10.
- **Shift+drag** to draw a horizontal or vertical line.
- **Ctrl+drag** to draw an arrow; the pointer changes to an upward arrow.
- Press **Ctrl+Z** to undo and **Ctrl+Y** to redo.
- Choose a history limit of 10, 20, 50, or 100 operations.
- Paint color and stroke width are retained between launches.

### Other features

- Hover to reveal the frame and window buttons.
- Change the capture frame color with an alpha-aware color picker.
- Change the capture window background color.
- Background alpha has a minimum value of 1 so transparent margins remain draggable; selecting 0 is normalized to 1.
- Drag any edge or corner to expand the canvas. New space is filled with the selected background color.
- Selecting an image or text item brings it to the front within its layer group.
- Drag the **Layers** panel entries to reorder paint, image, and text layer groups. The order is retained between launches, and the panel is excluded from copied and saved output.
- When the paint layer is above image or text layers, paint input takes priority in paint mode.
- Press **Esc** to close the current editor or cancel area selection. The application continues running in the notification area.
- Minimize the capture editor.
- Use the **mouse wheel** to change capture-window opacity, including near-transparent values.

## Usage

1. Launch **ScreenCapture.exe**.
2. Drag to select and capture an area.
3. Right-click and choose **Add Text**, or press **Alt** to enter paint mode.
4. Press **Ctrl+C** to copy the result.
5. Paste it into Word, PowerPoint, or another application.

## Display language

- Right-click menus, save dialogs, result messages, and editing tooltips follow the Windows display language and support English and Japanese.
- Unsupported display languages fall back to English.
- Both languages are included in the same application package, so no language-specific reinstall is required.

## Distribution

- The executable published directly through GitHub Releases is currently unsigned.
- Microsoft Store submission packages can be generated as MSIX. Packages that pass Store certification are signed by Microsoft and can be installed and updated safely through the Store.
- See [Microsoft Store distribution](docs/MICROSOFT_STORE.md) for submission instructions.

## Keyboard shortcuts

| Shortcut | Action |
|---|---|
| **Ctrl+C** | Copy to the clipboard |
| **Ctrl+S** | Save as an image |
| **Ctrl+V** | Paste an image |
| **Ctrl+click while selecting a capture area** | Capture the clicked window |
| **Alt** | Turn paint mode on or off |
| **Ctrl while painting** | Turn arrow mode on or off |
| **Shift+drag while painting** | Draw a horizontal or vertical line |
| **Shift+drag while resizing an image** | Preserve the aspect ratio |
| **Ctrl+Z** | Undo |
| **Ctrl+Y** | Redo |
| **Mouse wheel** | Change opacity |
| **Esc** | Close the editor or cancel area selection |

## Hotkey settings

- Right-click the notification-area icon.
- Select **Hotkey Settings...** to enable or change the global hotkey.
- Select **Third-party Licenses...** to view the licenses for the embedded font and color picker.
- When enabled, the configured hotkey starts a new capture.

## Technology

- .NET 8.0
- WPF (Windows Presentation Foundation)
- C#
- PixiEditor.ColorPicker with alpha support

## Development environment

- Visual Studio 2022
- Windows 10 or Windows 11

## Developer documentation

- [Codex working guide](AGENTS.md): rules and verification requirements for Codex changes
- [Development guide](DEVELOPMENT.md): setup, branch workflow, builds, and manual regression checks
- [Architecture](docs/ARCHITECTURE.md): runtime flow, coordinates, layers, settings, and known constraints
- [Code signing and release operations](docs/CODE_SIGNING.md): initial releases, SignPath application, and the post-approval signing flow
- [Microsoft Store distribution](docs/MICROSOFT_STORE.md): MSIX creation, Partner Center, and Store submission
- [Microsoft Store update playbook](docs/STORE_UPDATE_PLAYBOOK.md): preparing fixes, versions, packages, and Store updates from a new task
- [Icon update guide](ScreenCapture/ICON_SETUP.md): updating the executable and notification-area icons

## Build

```powershell
# Clone the repository
git clone https://github.com/sakusan393/ScreenCapture.git
cd ScreenCapture

# Build
dotnet build ScreenCapture.sln --configuration Debug

# Run
dotnet run --project ScreenCapture/ScreenCapture.csproj

# Publish the executable
dotnet publish ScreenCapture/ScreenCapture.csproj --configuration Release
# Output: ScreenCapture\bin\Release\net8.0-windows\win-x64\publish\ScreenCapture.exe

# Build the Microsoft Store MSIX submission package
.\scripts\Build-StoreMsix.ps1
```

## License

ScreenCapture is released under the [MIT License](LICENSE). Personal and commercial use, modification, and redistribution are permitted subject to the license terms.

The embedded Noto Sans JP font is provided under the SIL Open Font License 1.1, and PixiEditor.ColorPicker is provided under the MIT License. See [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md) or select **Third-party Licenses...** from the notification-area menu.

## Code signing policy

Until a signing service is available, GitHub Release notes identify directly distributed executables as unsigned. Microsoft signs the Store package after certification. If the project is approved by SignPath Foundation in the future, signed GitHub Release artifacts will use certificates provided through [SignPath.io](https://signpath.io/) and [SignPath Foundation](https://signpath.org/).

- Committer and reviewer: [393](https://393.bz/) ([sakusan393](https://github.com/sakusan393))
- Approver: [393](https://393.bz/) ([sakusan393](https://github.com/sakusan393))
- Details: [Code signing policy](CODE_SIGNING_POLICY.md)
- [Privacy policy](PRIVACY.md): ScreenCapture does not transmit information to networked systems.

## Bug reports and feature requests

Please use [GitHub Issues](https://github.com/sakusan393/ScreenCapture/issues).

## Author

Website: [393](https://393.bz/)

GitHub: [@sakusan393](https://github.com/sakusan393)
