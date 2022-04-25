using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Ya.D.Helpers;
using Ya.D.Models;

namespace Ya.D.ViewModels
{
    public class VideoViewModel : BaseItemsViewModel
    {
        public DelegateCommand<DiskItem> PlayCMD { get; set; }
        public DelegateCommand<DiskItem> AddToPlaylistCMD { get; set; }
        public DelegateCommand<DiskItem> RemoveFromPlaylistCMD { get; set; }

        public VideoViewModel()
        {
            Items = new DiskItemIncrementalLoading(DiskMediaType.video);
            PlayCMD = new DelegateCommand<DiskItem>(async i => await TryToPlayAsync(i));
            DownloadCMD = new DelegateCommand<DiskItem>(async i => await DownloadItem(i));
            DeleteCMD = new DelegateCommand<DiskItem>(async i => await DeleteItem(i));
            AddToPlaylistCMD = new DelegateCommand<DiskItem>(async i => await AddToPlaylist(i));
            RemoveFromPlaylistCMD = new DelegateCommand<DiskItem>(async i => await RemoveFromPlaylist(i));
        }

        public void GotoPlayer() =>
            NavigationService.Navigate(typeof(Views.MediaPlayer));

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            return Task.Factory.StartNew(async () =>
            {
                // await GetItemsAsync(DiskMediaType.video);
                await base.OnNavigatedToAsync(parameter, mode, state);
            }, CancellationToken.None, TaskCreationOptions.AttachedToParent, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override Task ItemChanged()
            => NavigationService.NavigateAsync(typeof(Views.DetailPage), new DetailsData { ItemToSelect = SelectedItem, Items = Items.ToList() });

        public async Task AddVideo()
        {
            await SelectAndUpload(new CancellationTokenSource(), FileTypes.Extensions.Where(i => i.Key.StartsWith("video")).Select(i => i.Key).ToList());
        }

        public async Task TakeAVideo()
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;
            var video = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Video);
            if (video == null)
            {
                return;
            }

            await SelectDiskFolderAndUpload(new List<StorageFile> { video }, new CancellationTokenSource());
            await video.DeleteAsync();
        }
    }
}
