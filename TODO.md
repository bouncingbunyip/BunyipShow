# BunyipShow TODO

- ## Fixed Size Rolling
    If the daily log files get too big, say larger than 5MB, consider adding fixed size rolling to Logger.cs
    The filename would become YYYY-MM-DD-bunyipshow-XXX.log where XXX is an incrementing counter starting at 001
---
- ## Thread Safety in PreloadBuffer
    Since a slideshow usually loads images on a background thread while the UI displays them on the main thread, `Queue<PreloadImage>` in PreloadBuffer might eventually cause a "Collection was modified" crash. The Suggestion: Consider using System.Collections.Concurrent.ConcurrentQueue<T>. It handles the "locking" for you automatically.
---
- ## Working with large image sets
    `Directory.GetFiles(...).ToList()` (used in ScanImages) is "eager." It waits until it finds every single file in target folder before giving you the list. If a user points this at a folder with 50,000 photos, the app will freeze for several seconds.  Consider using `Directory.EnumerateFiles`. It is "lazy"—it returns files one by one, allowing your UI to start showing the first photo while the rest are still being found.
---
- ## UI Responsiveness (Thread Blocking)
    In SlideshowForm.cs, the `FillPreloadBuffer` method is called on the UI Thread.
    The Issue: `ImageLoader.TryLoad` performs heavy disk I/O and bitmap decoding. If an image is large or on a slow drive, the UI will freeze (hitch) during the load, making the transition feel laggy.
    The Recommendation: Offload preloading to a background thread using Task.Run. This allows the UI to stay responsive while the next image prepares in the background.
---
- ## Memory Efficiency
    In ShowNextImage, the following line:
    `this.BackgroundImage = (Bitmap)loaded.Bitmap.Clone();`
    The Issue: By cloning the bitmap, the code is creating a second copy of the same image in RAM. Since the original is already in the PreloadBuffer, this effectively doubles the memory usage for that slide.
    The Fix: Assign the bitmap directly without cloning. Just don't dispose of the PreloadImage object until the next slide is ready to be shown.
---
- ## Efficient Randomization - DONE
    The current shuffling logic in GetNextImagePath uses `.OrderBy(x => _rng.Next())`.
    The Issue: This "Linq Shuffle" is easy to write but inefficient for large lists because it creates many temporary objects for the garbage collector to clean up.
    The Better Way: Use a Fisher-Yates Shuffle to reorder your _images list in-place once, then simply iterate through it sequentially.
    This should be complete now.
---
- ## ConcurrentQueue
    In PreloadBuffer.cs, change `Queue<PreloadImage>` to `ConcurrentQueue<PreloadImage>`
---
- ## Cleanup Optimization
    Consider removing any other BunyipShow processes that are currently running in the background with something like the following:
`private static void RunPreview(string[] args, Config config, System.Collections.Generic.List<string> images)`
{
    `// Clean up any other preview processes to prevent overlaps`
   `foreach (var p in System.Diagnostics.Process.GetProcessesByName``("BunyipShow")`
               ` .Where(p => p.Id != Environment.ProcessId))`
   ` {`
       ` try { p.Kill(); } catch { /* Ignore if already exiting */ }`
    `}`
    `Logger.Log("Preview mode requested.");`
---
- ## Regex Timeout
Consider adding a Regex Timeout. This prevents a "Malicious" folder name from causing "Catastrophic Backtracking" (where the CPU gets stuck in an infinite loop trying to match a weird name).  In ImageLoader.cs, you can update your Regex.IsMatch calls to include a small timeout:
`// This tells Windows: "If you can't match this name in 100ms, give up."`
`if (!Regex.IsMatch(folderPath, config.FolderFilters.IncludeRegex, `
`    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)))`
`{`
`    return false;`
`}`
 
---
- ## Preview image
   The little preview window/screen shown in Screen Saver Settings should show the preview.png file (if possible)
---
- ## Filter out non-images which have supported file extensions. - DONE
   For example, Files that start with '._' should be filtered out and not loaded.
   Implemented in commit 460f6e2

---
- ## Possible stutter
   When the preload buffer runs out, and code starts refilling buffer, there might be a stutter in the display of the images.  Once I have seen the "wait" cursor between images, and the images being displayed are fairly small (not more than 500KB)
---
- ## Ensure purging old logs
   Logs that are older than XX days should be deleted.  Ensure that they are.
---
- ## What happens when two folders have images with the same file names?
---
- ## Multiple image folders - DONE
    Currently we only load iamges from one folder.  Add support to load from multiple folders.
    config.json has something like:
    `"ImageRootFolder": "E:\\path\\to\\images"`
    to something like: 
    `"ImageRootFolder": "C:\\path\\to\\images, C:\\anotherPath\\to\\images"`
    This was implmented in commit 44f13b0

- ## Image Transitions
    Implement image transitions.  First one to implement is a cross fade.  After that look at a slide-push, then Ken Burns.

- ## Clock
    Add an optional clock, can be configured in config.json

- ## Metadata
    Add optional metadata, path & filename to be displayed with image.
    When you implement this, consider creating a helper method called RenderImageWithMetadata(string path). This method would:
    Load the image.
    If config.Overlay.ShowPath is true, draw the Path.GetFileName(path) onto the bottom of that specific image.
    Return a single Bitmap that contains both the photo and the text.
    This way, when you pass that single Bitmap to your transition engine (like a Cross-Fade), the text is "baked in" and moves or fades perfectly with the picture.

- ## Convert magic strings to enum
    In Config.cs DisplayOrder is a magic string, it should be an enum with the values of "random", "sequential"

- ## Unit tests
    Build out unit tests to ensure that:
     * file and folder level regex for include and exclude are working.
     * Image size filters are working
     * Image display toggle between random and sequential is working
     * 
