using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BunyipShow.Core;

using FormsTimer = System.Windows.Forms.Timer;

namespace BunyipShow
{
    public partial class SlideshowForm : Form
    {
        private readonly Config _config;
        private readonly List<string> _images = new();
        private readonly PreloadBuffer _preloadBuffer;
        private readonly FormsTimer _timer;
        private Point? _initialMousePosition;
        private bool _mouseExitEnabled => _config.MouseExitThresholdPixels > 0;
        private readonly bool _isPreviewMode;
        private bool _isLoaded = false;

        // Randomization / sequential
        private List<string> _randomQueue = new();
        private int _sequentialIndex = 0;
        private bool IsRandom => string.Equals(_config.DisplayOrder, "random", StringComparison.OrdinalIgnoreCase);
        private readonly Random _rng = new Random();

        public SlideshowForm(Config config, List<string> images, bool isPreviewMode = false)
        {
            // Hide the mouse cursor for slideshow
            if (!isPreviewMode)
            {
                Cursor.Hide();
            }

            _config = config;
            _images = images ?? new List<string>();
            _isPreviewMode = isPreviewMode;

            // Initialize mouse baseline immediately
            if (_mouseExitEnabled && !_isPreviewMode)
            {
                _initialMousePosition = Cursor.Position;
            }

            // Form appearance
            this.BackColor = ColorTranslator.FromHtml(_config.BackgroundColor ?? "#000000");
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.KeyDown += (s, e) =>
            {
                if (_isLoaded)
                    Close();
            };

            this.MouseDown += (s, e) =>
            {
                if (_isLoaded)
                    Close();
            };

            // Preload buffer
            long maxSingleBytes = _config.MaxIndividualImageSizeMB * 1024L * 1024L;
            long maxTotalBytes = _config.MaxImageMemoryMB * 1024L * 1024L;
            _preloadBuffer = new PreloadBuffer(maxTotalBytes);

            FillPreloadBuffer(_config.PreloadImages > 0 ? _config.PreloadImages : 3, maxSingleBytes);

            // Show first image immediately
            ShowNextImage();

            // Timer for slideshow
            _timer = new FormsTimer();
            _timer.Interval = (_config.DisplayDurationSeconds > 0 ? _config.DisplayDurationSeconds : 15) * 1000;
            _timer.Tick += (s, e) => ShowNextImage();
            _timer.Start();

            if (_mouseExitEnabled)
                this.MouseMove += SlideshowForm_MouseMove;

            this.Load += (s, e) => _isLoaded = true;
        }

        private void SlideshowForm_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_mouseExitEnabled) return;

            if (_initialMousePosition == null)
            {
                _initialMousePosition = Cursor.Position;
                return;
            }

            int dx = Math.Abs(Cursor.Position.X - _initialMousePosition.Value.X);
            int dy = Math.Abs(Cursor.Position.Y - _initialMousePosition.Value.Y);

            if (dx > _config.MouseExitThresholdPixels || dy > _config.MouseExitThresholdPixels)
            {
                Logger.Log("Mouse moved beyond threshold, exiting slideshow.");
                if (_isLoaded)
                    Close();
            }
        }

        private void FillPreloadBuffer(int preloadCount, long maxSingleBytes)
        {
            for (int i = 0; i < preloadCount; i++)
            {
                string? path = GetNextImagePath();
                if (path == null) break;

                var loaded = ImageLoader.TryLoad(path, maxSingleBytes, Logger.Log);
                if (loaded != null)
                {
                    _preloadBuffer.Add(loaded);
                    Logger.Log($"Preloaded image: {path}");
                }
            }

            Logger.Log($"Preload buffer count: {_preloadBuffer.Count}");
        }

        private string? GetNextImagePath()
        {
            if (_images.Count == 0) return null;

            if (IsRandom)
            {
                if (_randomQueue.Count == 0)
                {
                    // refill and shuffle
                    _randomQueue = _images.ToList();
                    _randomQueue = _randomQueue.OrderBy(x => _rng.Next()).ToList();
                }

                string next = _randomQueue[0];
                _randomQueue.RemoveAt(0);
                return next;
            }
            else
            {
                string next = _images[_sequentialIndex % _images.Count];
                _sequentialIndex++;
                return next;
            }
        }

        private void ShowNextImage()
        {
            // 1. Dequeue the preloaded image
            using var loaded = _preloadBuffer.Dequeue();

            if (loaded != null)
            {
                // 2. Dispose of the OLD background before setting the new one
                var oldImage = this.BackgroundImage;

                // 3. Set directly (no .Clone())
                this.BackgroundImage = new Bitmap(loaded.Bitmap);
                this.BackgroundImageLayout = ImageLayout.Zoom;

                oldImage?.Dispose();
            }

            // 4. Fill the buffer on a BACKGROUND thread to prevent UI stutter
            Task.Run(() => FillPreloadBuffer(1, _config.MaxIndividualImageSizeMB * 1024L * 1024L));
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _preloadBuffer?.Dispose();
                this.BackgroundImage?.Dispose();

                // Restore cursor
                if (!_isPreviewMode)
                    Cursor.Show();
            }
            base.Dispose(disposing);
        }


    }
}