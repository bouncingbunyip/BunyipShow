using System;
using System.Collections.Generic;
using System.Linq;

namespace BunyipShow.Core
{
    public sealed class PreloadBuffer : IDisposable
    {
        private readonly Queue<PreloadImage> _buffer = new();
        private readonly long _maxBytes;
        private long _currentBytes;

        public PreloadBuffer(long maxBytes)
        {
            _maxBytes = maxBytes;
        }

        public int Count => _buffer.Count;
        public void Add(PreloadImage image)
        {
            Enqueue(image);
        }
        public void Enqueue(PreloadImage image)
        {
            _buffer.Enqueue(image);
            _currentBytes += image.EstimatedBytes;
            TrimIfNeeded();
        }

        public PreloadImage? Dequeue()
        {
            if (_buffer.Count == 0) return null;

            var img = _buffer.Dequeue();
            _currentBytes -= img.EstimatedBytes;
            return img;
        }

        private void TrimIfNeeded()
        {
            while (_currentBytes > _maxBytes && _buffer.Count > 0)
            {
                var old = _buffer.Dequeue();
                _currentBytes -= old.EstimatedBytes;
                old.Dispose();
            }
        }

        // --- NEW METHOD ---
        public PreloadImage? Get(int index)
        {
            if (_buffer.Count == 0) return null;

            // Convert queue to list temporarily for indexed access
            var list = _buffer.ToList();
            int safeIndex = index % list.Count; // wrap around
            return list[safeIndex];
        }

        public void Dispose()
        {
            while (_buffer.Count > 0)
                _buffer.Dequeue()?.Dispose();
        }
    }
}