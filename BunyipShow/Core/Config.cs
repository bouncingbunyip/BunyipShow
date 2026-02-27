using System.Collections.Generic;

namespace BunyipShow.Core;

public class Config
{
    // Paths
    public PathsConfig Paths { get; set; } = new();

    // Display options
    public int DisplayDurationSeconds { get; set; } = 15;
    public string DisplayOrder { get; set; } = "random"; // "random" or "sequential"
    public string BackgroundColor { get; set; } = "#000000";
    public OverlayConfig Overlay { get; set; } = new();

    public ClockConfig Clock { get; set; } = new();

    // Mouse exit
    public int MouseExitThresholdPixels { get; set; } = 10;

    // Preload / memory
    public int PreloadImages { get; set; } = 3;
    public int MaxIndividualImageSizeMB { get; set; } = 64;
    public int MaxImageMemoryMB { get; set; } = 256;
    public int MaxScannedFiles { get; set; } = 10000; // default limit

    // Image filters
    public ImageFiltersConfig ImageFilters { get; set; } = new();
    public FolderFiltersConfig FolderFilters { get; set; } = new();

    // Logging
    public bool EnableLogging { get; set; } = true;
}

public class PathsConfig
{
    public string ImageRootFolder { get; set; } = "";
}

public class OverlayConfig
{
    public bool ShowPath { get; set; } = true;
    public string FontFamily { get; set; } = "Consolas";
    public int FontSize { get; set; } = 24;
    public string FontColor { get; set; } = "#FFFFFF";
}
public class ClockConfig
{
    public bool ShowTime { get; set; } = true;
    public string FontFamily { get; set; } = "Consolas";
    public int FontSize { get; set; } = 24;
    public string FontColor { get; set; } = "#FFFFFF";
}


public class ImageFiltersConfig
{
    public int MinWidth { get; set; } = 0;
    public int MaxWidth { get; set; } = 0;
    public int MinHeight { get; set; } = 0;
    public int MaxHeight { get; set; } = 0;
    public int MinFileSizeKB { get; set; } = 0;
    public int MaxFileSizeKB { get; set; } = 0;
    public string FilenameIncludeRegex { get; set; } = "";
    public string FilenameExcludeRegex { get; set; } = "";
}

public class FolderFiltersConfig
{
    public string IncludeRegex { get; set; } = "";
    public string ExcludeRegex { get; set; } = "";
}