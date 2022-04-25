using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Ya.D.Models;

namespace Ya.D.Services
{
    public class MediaPlayerService
    {
        private static readonly Lazy<MediaPlayerService> _instance = new Lazy<MediaPlayerService>(() => new MediaPlayerService());
        private MediaSource _mediaSource;
        private MediaPlaybackItem _mediaPlaybackItem;

        private DiskItem _currentlyPlaying;
        private PlayList _currentPlayList;

        public static MediaPlayerService Service => _instance.Value;
        public MediaPlayer Player { get; }

        private MediaPlayerService()
        {
            Player = new MediaPlayer { AutoPlay = true };
            Player.MediaEnded += _mediaPlayer_MediaEnded;
        }

        private async void _mediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            if (_currentlyPlaying == null && sender.PlaybackSession.CanPause)
            {
                sender.Pause();
                return;
            }
            if (_currentPlayList != null && _currentPlayList.DiskItems.Count > 0 && _currentPlayList.DiskItems.Contains(_currentlyPlaying))
            {
                var current = _currentPlayList.DiskItems.IndexOf(_currentlyPlaying);
                if (current < 0)
                {
                    sender.Pause();
                    return;
                }
                _currentlyPlaying = current < _currentPlayList.DiskItems.Count - 1 ?
                        _currentPlayList.DiskItems.Skip(current + 1).Take(1).FirstOrDefault() :
                        _currentPlayList.DiskItems.FirstOrDefault();
                await PlayDiskItemAsync(_currentlyPlaying);
            }
        }

        private void PlayByURL(Uri mediaURL, string title, bool audio)
        {
            if (Player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                Player.Pause();
            }

            try
            {
                _mediaSource = MediaSource.CreateFromUri(mediaURL);
                _mediaPlaybackItem = new MediaPlaybackItem(_mediaSource);
                _mediaPlaybackItem.AutoLoadedDisplayProperties = AutoLoadedDisplayPropertyKind.MusicOrVideo;
                var props = _mediaPlaybackItem.GetDisplayProperties();
                if (audio)
                {
                    props.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/image_sound_wave.png"));
                    props.Type = Windows.Media.MediaPlaybackType.Music;
                    props.MusicProperties.Title = title;
                }
                else
                {
                    props.Type = Windows.Media.MediaPlaybackType.Video;
                    props.VideoProperties.Title = title;
                }
                _mediaPlaybackItem.ApplyDisplayProperties(props);
                Player.Source = _mediaPlaybackItem;
                Player.Play();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> PlayPlaylistAsync(PlayList playlist)
        {
            if (playlist == null)
            {
                return false;
            }

            _currentPlayList = playlist;
            if (_currentPlayList.DiskItems.Count == 0)
            {
                return false;
            }

            _currentlyPlaying = _currentPlayList.DiskItems.FirstOrDefault();
            return await PlayDiskItemAsync(_currentlyPlaying);
        }

        public async Task<bool> PlayDiskItemAsync(DiskItem diskItem)
        {
            if (diskItem == null || !diskItem.MimeType.StartsWith("audio") && !diskItem.MimeType.StartsWith("video"))
            {
                return false;
            }

            var url = await DiskService.Client.GetDownloadURL(diskItem.Path);
            if (url.IsError())
            {
                return false;
            }

            var nameParts = diskItem.DisplayName.Split('.');
            _currentlyPlaying = diskItem;
            PlayByURL(new Uri(url.URL), string.Join(" ", nameParts.Take(nameParts.Length - 1)), _currentlyPlaying.MimeType.StartsWith("audio"));
            return true;
        }
    }
}
