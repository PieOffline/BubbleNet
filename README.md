# BubbleNet 🫧
**QuickShare, in a bubble!**

A modern, dark-themed local network file sharing application for Windows. Share files, links, text, and screenshots across your local network using easy-to-remember food word codes.

![BubbleNet Screenshot](docs/screenshot.png)

## ✨ Features

- **Dark Mode UI** - Beautiful modern dark theme throughout
- **Food Word Codes** - Remember IP addresses as food words (e.g., `Apple/Blackberry/Citrus`)
- **Multiple Transfer Types**:
  - 📁 Files - Send any file type
  - 📝 Text - Share quick messages
  - 🔗 Links - Share URLs (with auto-open option)
  - 📸 Screenshots - Capture and send your screen
- **Receive Controls**:
  - Auto-Deny toggle to block all incoming transfers
  - Auto-Open toggle to automatically open received links in browser
- **Sound Effects** - Audio feedback for various actions
- **Transfer History** - View received items with timestamp and sender info

## 🚀 Installation

### Prerequisites
- Windows 10/11
- Visual Studio 2022
- .NET 8.0 SDK or later

### Building from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/PieOffline/BubbleNet.git
   cd BubbleNet
   ```

2. **Open in Visual Studio 2022**
   - Open `BubbleNet.sln` in Visual Studio 2022
   - Or double-click the `.sln` file

3. **Restore NuGet packages**
   - Visual Studio will automatically restore packages
   - Or run: `dotnet restore`

4. **Build the project**
   - Press `Ctrl+Shift+B` or go to `Build > Build Solution`
   - Or run: `dotnet build`

5. **Run the application**
   - Press `F5` to run with debugging
   - Or press `Ctrl+F5` to run without debugging
   - Or run: `dotnet run --project BubbleNet`

### Creating a Release Build

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

The executable will be in `BubbleNet/bin/Release/net8.0-windows/win-x64/publish/`

## 📖 How to Use

### Getting Started

1. **Launch BubbleNet** - Open the application
2. **Click "Mesh Me"** - This opens port 16741 (or 6741 as fallback) for connections
3. **Note your Word Code** - You'll see a 3-word code like `Apple/Bacon/Cheese`
4. **Share your Word Code** - Tell others your word code so they can send to you

### Sending Data

1. **Enter the recipient's Word Code** in the "Recipient Word Code" field
   - Format: `Word1/Word2/Word3` (e.g., `Banana/Carrot/Date`)
2. **Choose what to send**:
   - **Send File**: Click to browse and select a file
   - **Send Screenshot**: Captures your screen and sends it
   - **Send Text**: Type your message and click "Send Text"
   - **Send Link**: Enter a URL and click "Send Link"

### Receiving Data

- All received items appear in the "Received" section on the right
- Each item shows:
  - Type icon and description
  - Action buttons (Open, Copy, Download)
  - Sender's word code
  - Timestamp

### Toggles

- **Auto-Deny**: When ON, automatically rejects all incoming transfers
- **Auto-Open**: When ON, automatically opens received links in your browser

## 🔧 Technical Details

### Ports Used
- **Primary**: 16741
- **Fallback**: 6741 (if primary is in use)

### Network
- Works on local network (same subnet)
- Uses TCP for reliable transfers
- Word codes map to IP octets using food names

### Word Code System
- 255 unique food words (one for each IP octet value 1-255)
- Format: `Word2/Word3/Word4` (based on last 3 IP octets)
- Examples:
  - `Apple/Blackberry/Citrus` = x.1.13.35
  - `Banana/Cheese/Date` = x.4.27.55

## 🔒 Security Notes

- BubbleNet only works on your local network
- No authentication is built-in - use the Auto-Deny feature when not expecting transfers
- Files are transferred directly - no cloud storage involved

## 🐛 Troubleshooting

**Can't connect to other users?**
- Ensure both users are on the same local network
- Check that Windows Firewall allows BubbleNet
- Try the fallback port (6741) if 16741 is blocked

**Word code not working?**
- Verify the word code is entered exactly (case-insensitive)
- Check that the target device has BubbleNet running with "Mesh Me" active

**No sound effects?**
- BubbleNet uses Windows system sounds
- Ensure system sounds are not muted

## 📝 License

This project is open source. Feel free to modify and distribute.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

---

Made with 🫧 by the BubbleNet Team
