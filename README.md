# BunyipShow 🐾
**A High-Performance Windows Screen Saver Engine for Massive Photo Libraries.**

BunyipShow is a **.NET 8.0** photo slideshow screensaver designed to handle exceptionally large image collections (500,000+ files) across multiple drives without hanging or crashing. Unlike standard screensavers, it uses lazy-loading enumeration and intelligent filtering to provide a smooth experience even on slower or heavily populated storage volumes.

## 🌟 Key Features

* **⚡ Blazing Fast Scanning**: Uses `Directory.EnumerateFiles` instead of traditional indexing. This allows the slideshow to start instantly, even if your root folder contains hundreds of thousands of files.
* **📸 Smart Auto-Rotation**: Automatically detects and applies EXIF orientation flags using modern C# pattern matching. Vertical photos taken on mobile devices will always appear right-side up.
* **🛡️ Zombie Process Protection**: Includes a built-in Watchdog Timer that monitors the Windows Screen Saver parent window. If the Preview window or Settings dialog is closed, BunyipShow cleans up its own process immediately.
* **🔍 Robust Regex Filtering**: Advanced `Include` and `Exclude` filters for folders and filenames. Easily target specific people, years, or events (e.g., `(?i).*Amara.*`).
* **💾 Memory Efficient**: Uses an independent Bitmap buffer strategy to load high-resolution images without locking the original files on disk.

## 🚀 Getting Started

### Prerequisites
* **Windows 10 or 11**
* **.NET 8.0 Runtime** (Required for modern C# pattern matching and performance optimizations)

### Installation
1. Compile the project to an `.exe` targeting `net8.0-windows`.
2. Rename `BunyipShow.exe` to `BunyipShow.scr`.
3. Right-click the `.scr` file and select **Install**, or move it to `C:\Windows\System32`.

## ⚙️ Configuration

The application looks for a `config.json` file in your AppData folder (`%AppData%\BunyipShow\config.json`).  Please take a look at config.example.json for a full example.
For more information about all the available configuration options look in [Configuration.md](Configuration.md)

```json
{
  "ImageSourceFolder": [ "E:\\Photos\\Archive" ],
  "MaxScannedFiles": 1000,
  "FolderFilters": {
    "IncludeRegex": "Amara|Emily",
    "ExcludeRegex": "Drafts|Temp"
  }
}