using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BunyipShow.Core
{
    public static class ImageLoader
    {
        private static readonly HashSet<string> SupportedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".tiff", ".webp" };

        private const int ExifOrientationId = 0x0112;

        public static PreloadImage? TryLoad(string path, long maxBytes, Action<string> log)
        {
            try
            {
                var fi = new FileInfo(path);
                if (!fi.Exists) return null;
                if (fi.Length > maxBytes) return null;

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var sourceImg = Image.FromStream(fs);

                // FIX: Apply rotation to the SOURCE image before creating the Bitmap
                // Metadata is lost during the "new Bitmap()" constructor
                ApplyExifRotation(sourceImg);

                Bitmap bmp = new Bitmap(sourceImg);
                return new PreloadImage(path, bmp);
            }
            catch (Exception ex)
            {
                log($"Failed to load image {path}: {ex.Message}");
                return null;
            }
        }

        public static List<string> ScanImages(Config config)
        {
            var images = new List<string>();
            string root = config.Paths?.ImageRootFolder ?? "";
            int maxFiles = config.MaxScannedFiles > 0 ? config.MaxScannedFiles : 1000;

            Logger.Log($"Scanning images in folder: {root}");

            if (!Directory.Exists(root))
            {
                Logger.Log($"Image folder does not exist: {root}");
                return images;
            }

            try
            {
                // FIX: Use EnumerateFiles to avoid loading 700,000 strings into memory at once
                var fileQuery = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);

                foreach (var file in fileQuery)
                {
                    // Stop once we've found enough ELIGIBLE images
                    if (images.Count >= maxFiles) break;

                    if (!SupportedExtensions.Contains(Path.GetExtension(file)))
                        continue;

                    if (PassFilters(file, config))
                    {
                        images.Add(file);
                        // Log every 100th image to show progress without flooding the log
                        if (images.Count % 100 == 0)
                            Logger.Log($"Found {images.Count} eligible images so far...");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error scanning images in {root}: {ex.Message}");
            }

            Logger.Log($"Final eligible images: {images.Count}");
            return images;
        }

        private static bool PassFilters(string path, Config config)
        {
            try
            {
                var fi = new FileInfo(path);

                // Filename filters
                if (!string.IsNullOrEmpty(config.ImageFilters.FilenameIncludeRegex) &&
                    !Regex.IsMatch(fi.Name, config.ImageFilters.FilenameIncludeRegex, RegexOptions.IgnoreCase))
                    return false;

                // Folder filters
                var folderPath = Path.GetDirectoryName(path) ?? "";
                if (!string.IsNullOrEmpty(config.FolderFilters.IncludeRegex))
                {
                    // FIX: Added RegexOptions.IgnoreCase by default for "Windows-style" matching
                    if (!Regex.IsMatch(folderPath, config.FolderFilters.IncludeRegex, RegexOptions.IgnoreCase))
                        return false;
                }

                if (!string.IsNullOrEmpty(config.FolderFilters.ExcludeRegex) &&
                    Regex.IsMatch(folderPath, config.FolderFilters.ExcludeRegex, RegexOptions.IgnoreCase))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ApplyExifRotation(Image img)
        {
            try
            {
                if (img.PropertyIdList == null || !img.PropertyIdList.Contains(ExifOrientationId))
                    return;

                var prop = img.GetPropertyItem(ExifOrientationId);

                // FIX: Explicitly check that prop and prop.Value are not null
                if (prop != null && prop.Value != null)
                {
                    int orientation = BitConverter.ToUInt16(prop.Value, 0);

                    RotateFlipType flipType = orientation switch
                    {
                        3 => RotateFlipType.Rotate180FlipNone,
                        6 => RotateFlipType.Rotate90FlipNone,
                        8 => RotateFlipType.Rotate270FlipNone,
                        _ => RotateFlipType.RotateNoneFlipNone
                    };

                    if (flipType != RotateFlipType.RotateNoneFlipNone)
                        img.RotateFlip(flipType);

                    // Remove the property so it's not applied again by accident
                    img.RemovePropertyItem(ExifOrientationId);
                }
            }
            catch
            {
                // ignore bad EXIF metadata
            }
        }
    }
}