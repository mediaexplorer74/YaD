using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Navigation;
using Ya.D.Models;
using Ya.D.Services;
using Ya.D.Views.Dialogs;

namespace Ya.D.ViewModels
{
    public class MediaPlayerViewModel : ViewModelBase
    {
        private DiskItem _currentItem;
        private PlayList _playList;


        public DiskItem CurrentItem { get => _currentItem; set => Set(ref _currentItem, value); }
        public PlayList CurrentPlaylist { get => _playList; set { Set(ref _playList, value); CurrentItem = _playList.DiskItems.FirstOrDefault(); } }
        public ObservableCollection<PlayList> AllPlaylists { get; set; } = new ObservableCollection<PlayList>();

        public DelegateCommand<DiskItem> PlayCMD { get; }
        public DelegateCommand<DiskItem> RemoveFromListCMD { get; }
        public DelegateCommand<PlayList> PlayAListCMD { get; }

        public MediaPlayerViewModel()
        {
            PlayCMD = new DelegateCommand<DiskItem>(async item => { await PlayItem(item); CurrentItem = item; });
            PlayAListCMD = new DelegateCommand<PlayList>(async item => { await PlayAListItem(item); CurrentPlaylist = item; });
            RemoveFromListCMD = new DelegateCommand<DiskItem>(async item => { await RemoveFromList(item); });
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter != null)
            {
                if (parameter is DiskItem)
                {
                    CurrentItem = parameter as DiskItem;
                    CurrentPlaylist = new PlayList() { Name = "One song list", DiskItems = new List<DiskItem>() { CurrentItem } };
                }
                else if (parameter is PlayList)
                {
                    CurrentPlaylist = parameter as PlayList;
                    if (CurrentPlaylist != null && CurrentPlaylist.DiskItems.Count == 0)
                    {
                        CurrentPlaylist = await DataProvider.Instance.GetPlaylistByIDAsync(CurrentPlaylist.ID);
                    }

                    CurrentItem = CurrentPlaylist.DiskItems.FirstOrDefault();
                }
                if (CurrentItem != null)
                {
                    await MediaPlayerService.Service.PlayDiskItemAsync(CurrentItem);
                }
            }
            // playlists 
            AllPlaylists.Clear();
            DataProvider.Instance.GetPlaylists(true).ForEach(l => AllPlaylists.Add(l));
            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        public async Task PlayItem(DiskItem item)
            => await MediaPlayerService.Service.PlayDiskItemAsync(item);

        public async Task RemoveFromList(DiskItem item)
        {
            await Task.Delay(100);
        }

        public async Task PlayAListItem(PlayList item)
            => await MediaPlayerService.Service.PlayPlaylistAsync(item);

        public async Task AddPlayList()
        {
            var dialog = new AddPlaylistDialog();
            await dialog.ShowAsync();
            if (dialog.Result == null)
            {
                return;
            }

            AllPlaylists.Add(dialog.Result);
        }


    }
}