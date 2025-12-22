using Imgur.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Controls;

namespace Imgur.Contracts
{
    public interface IMediaPlayerService
    {

        void PlayMedia (MediaPlayerElement mediaPlayer, Element element = null);

        void StopMedia(MediaPlayerElement mediaPlayer);

        void GoBackward();

        void GoForward();

        void SetTimeSpaces(int timespace);

    }
}
