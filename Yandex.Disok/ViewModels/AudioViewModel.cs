using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;
using Ya.D.Helpers;
using Ya.D.Models;

namespace Ya.D.ViewModels
{
    public class AudioViewModel : BaseItemsViewModel
    {
        private readonly Uri _imageFile = new Uri("ms-appx:///Assets/icons/icon_audio.png");
        private CancellationTokenSource _source = new CancellationTokenSource();

        public DelegateCommand<DiskItem> PlayCMD { get; set; }
        public DelegateCommand<DiskItem> AddToPlaylistCMD { get; set; }
        public DelegateCommand<DiskItem> RemoveFromPlaylistCMD { get; set; }

        public AudioViewModel()
        {
            Items = new DiskItemIncrementalLoading(DiskMediaType.audio, _imageFile);
            PageTitle = "Audio feed";
            PlayCMD = new DelegateCommand<DiskItem>(async i => await TryToPlayAsync(i));
            DownloadCMD = new DelegateCommand<DiskItem>(async i => await DownloadItem(i));
            DeleteCMD = new DelegateCommand<DiskItem>(async i => await DeleteItem(i));
            AddToPlaylistCMD = new DelegateCommand<DiskItem>(async i => await AddToPlaylist(i));
            RemoveFromPlaylistCMD = new DelegateCommand<DiskItem>(async i => await RemoveFromPlaylist(i));
        }

        public void GotoPlayer() =>
            NavigationService.Navigate(typeof(Views.MediaPlayer));

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            SelectedItem = null;
            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        public override Task ItemChanged()
            => NavigationService.NavigateAsync(typeof(Views.DetailPage), new DetailsData { ItemToSelect = SelectedItem, Items = Items.ToList() });

        public async Task AddAudio()
        {
            var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.MusicLibrary };
            FileTypes.Extensions.Where(i => i.Value.StartsWith("audio"))
                .Select(i => i.Key)
                .ToList()
                .ForEach(e => picker.FileTypeFilter.Add(e));
            var files = await picker.PickMultipleFilesAsync();
            if (files == null || files.Count == 0)
            {
                return;
            }

            if (_source.IsCancellationRequested)
            {
                _source = new CancellationTokenSource();
            }

            await SelectDiskFolderAndUpload(files, _source);
        }
    }
}
