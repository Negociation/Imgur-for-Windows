using Imgur.Models;
using System;

namespace Imgur.Uwp.Models
{
    /// <summary>
    /// POC payload for RootFrame fullscreen media player (newMode).
    /// </summary>
    public sealed class FullScreenMediaPayload
    {
        public Element Element { get; set; }
        public Media Post { get; set; }
        public TimeSpan Position { get; set; }
        public bool WasPlaying { get; set; }
        public Action FavoriteChanged { get; set; }
    }
}
