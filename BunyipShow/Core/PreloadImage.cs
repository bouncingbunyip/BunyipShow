// src/Core/PreloadImage.cs
using System.Drawing;

namespace BunyipShow.Core
{
    public sealed class PreloadImage : IDisposable
    {
        public string Path { get; }
        public Bitmap Bitmap { get; }
        public long EstimatedBytes { get; }

        public PreloadImage(string path, Bitmap bitmap)
        {
            Path = path;
            Bitmap = bitmap;

            // Get actual bits per pixel
            int bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
            EstimatedBytes = (long)bitmap.Width * bitmap.Height * (bpp / 8);
        }
        public void Dispose()
        {
            Bitmap.Dispose();
        }
    }
}