# Configuration Reference

The following settings are available in `config.json`. The file should be located in `%AppData%\BunyipShow\`.

### Paths

* **`ImageSourceFolder`** (string)
The one or more directories where the image scan looks for images.
    
    Examples:
    
    `"ImageSourceFolder": [ "C:\\Path\\To\\Photos" ]`
    
    `"ImageSourceFolder": [ "C:\\Path\\To\\Photos", "C:\\Path\\To\\AdditionalPhotos" ]`

### Display Settings

* **`DisplayDurationSeconds`** (int)
Duration in seconds to display each image before transitioning.
*Example:* `"DisplayDurationSeconds": 20`
* **`DisplayOrder`** (string)
The order in which images are shown. Use `"random"` or `"sequential"`.
*Example:* `"DisplayOrder": "random"`
* **`BackgroundColor`** (string)
Hexadecimal color code used for the background or letterboxing.
*Example:* `"BackgroundColor": "#1A1A1A"`

### Overlay Settings

These settings control the visibility and appearance of the image metadata (file path and name).

* **`Overlay:ShowPath`** (bool)
Toggles the display of the current image's file path and filename.
*Example:* `"Overlay": { "ShowPath": true }`

* **`Overlay:FontFamily`** (string)
The name of the font to use for the overlay text.
*Example:* `"Overlay": { "FontFamily": "Segoe UI" }`

* **`Overlay:FontSize`** (int)
The size of the font for the overlay text.
*Example:* `"Overlay": { "FontSize": 18 }`

* **`Overlay:FontColor`** (string)
The hex color code for the overlay text.
*Example:* `"Overlay": { "FontColor": "#FFFFFF" }`

### Clock Settings

These settings control the optional clock display.

* **`Clock:ShowTime`** (bool)
Toggles the display of the current time (HH:mm).
*Example:* `"Clock": { "ShowTime": true }

* **`Clock:FontFamily`** (string)
The name of the font to use for the clock.
*Example:* `"Clock": { "FontFamily": "Consolas" }

* **`Clock:FontSize`** (int)
The size of the font for the clock.
*Example:* `"Clock": { "FontSize": 24 }

* **`Clock:FontColor`** (string)
The hex color code for the clock text.
*Example:* `"Clock": { "FontColor": "#FFCC00" }

### Input Sensitivity

* **`MouseExitThresholdPixels`** (int)
The distance in pixels the cursor must move before the application exits.
*Example:* `"MouseExitThresholdPixels": 25`

### Performance & Memory

* **`PreloadImages`** (int)
Number of upcoming images to load into the background buffer to ensure smooth transitions.
*Example:* `"PreloadImages": 5`
* **`MaxIndividualImageSizeMB`** (int)
Maximum file size allowed for an individual image to be loaded.
*Example:* `"MaxIndividualImageSizeMB": 128`
* **`MaxImageMemoryMB`** (int)
The total RAM limit for the buffered image collection.
*Example:* `"MaxImageMemoryMB": 512`
* **`MaxScannedFiles`** (int)
The maximum number of valid files the scanner will find before concluding the search.
*Example:* `"MaxScannedFiles": 25000`

### Image Filters

* **`ImageFilters:MinWidth` / `MaxWidth**` (int)
Minimum and maximum horizontal resolution limits.
*Example:* `"ImageFilters": { "MinWidth": 1920, "MaxWidth": 8000 }`
* **`ImageFilters:MinHeight` / `MaxHeight**` (int)
Minimum and maximum vertical resolution limits.
*Example:* `"ImageFilters": { "MinHeight": 1080, "MaxHeight": 6000 }`
* **`ImageFilters:MinFileSizeKB` / `MaxFileSizeKB**` (int)
The allowed range for file sizes, in Kilobytes.
*Example:* `"ImageFilters": { "MinFileSizeKB": 500, "MaxFileSizeKB": 25000 }`
* **`ImageFilters:FilenameIncludeRegex`** (string)
Regex pattern for filenames to include.
*Example:* `"ImageFilters": { "FilenameIncludeRegex": "DSC_.*" }`
* **`ImageFilters:FilenameExcludeRegex`** (string)
Regex pattern for filenames to ignore.
*Example:* `"ImageFilters": { "FilenameExcludeRegex": ".*_thumb" }`

### Folder Filters

* **`FolderFilters:IncludeRegex`** (string)
Regex pattern used to include specific folder paths.
*Example:* `"FolderFilters": { "IncludeRegex": "202[3-4]" }`
* **`FolderFilters:ExcludeRegex`** (string)
Regex pattern used to skip specific folder paths entirely during the scan.
*Example:* `"FolderFilters": { "ExcludeRegex": "Temp|Private" }`

### Diagnostics

* **`EnableLogging`** (bool)
Enables or disables diagnostic logging.
*Example:* `"EnableLogging": true`