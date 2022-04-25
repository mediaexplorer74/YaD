using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Ya.D.Helpers;
using Ya.D.Models;
using Ya.D.Services;
using Ya.D.Services.SettingsServices;
using Ya.D.Views;
using Ya.D.Views.Dialogs;

namespace Ya.D.ViewModels
{
    public abstract class BaseItemsViewModel : ViewModelBase
    {
        private string _title = "Base feed";
        private DiskItem _selectedItem;

        public DiskMediaType MediaType { get; set; }
        public string PageTitle { get => _title; set => Set(ref _title, value); }
        public DiskItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (Set(ref _selectedItem, value))
                {
                    Task.Factory.StartNew(async () => await ItemChanged(), CancellationToken.None, TaskCreationOptions.None, DiskService.Client.Sync);
                }
            }
        }
        public DiskItemIncrementalLoading Items { get; set; }

        public DelegateCommand<DiskItem> DownloadCMD { get; set; }
        public DelegateCommand<DiskItem> DeleteCMD { get; set; }
        public DelegateCommand<DiskItem> GoToDetailsCMD { get; set; }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            Items.CancellationSource.Cancel();
            return base.OnNavigatingFromAsync(args);
        }

        public void GridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)(sender as GridView).ItemsPanelRoot;
            var actualState = Shell.Instance.CurrentState;
            if (actualState == null)
            {
                return;
            }

            int gridColumnWidth;
            switch (actualState.Name)
            {
                case "VisualStateNarrow":
                    gridColumnWidth = 3;
                    break;
                case "VisualStateNormal":
                    gridColumnWidth = 6;
                    break;
                default:
                    gridColumnWidth = 9;
                    break;
            }
            panel.ItemWidth = e.NewSize.Width / gridColumnWidth;
        }

        public virtual async Task ItemChanged()
        {
            await Task.CompletedTask;
        }

        public virtual async Task GoToDetails(DiskItem diskItem)
        {
            if (diskItem != null)
            {
                await NavigationService.NavigateAsync(typeof(DetailPage), new DetailsData { ItemToSelect = diskItem, Items = Items.ToList() });
            }
        }

        public virtual async Task<bool> DownloadItem(DiskItem diskItem)
        {
            if (diskItem == null)
            {
                return false;
            }

            var result = await DiskService.Instance.DownloadItem(diskItem);
            return result;
        }

        public virtual async Task<bool> DeleteItem(DiskItem diskItem)
        {
            if (diskItem == null)
            {
                return false;
            }

            var result = await DiskService.Instance.DeleteItem(diskItem, (e) => Items.Remove(e));
            return result;
        }

        public virtual async Task TryToPlayAsync(DiskItem diskItem)
        {
            if (diskItem == null)
            {
                return;
            }

            await NavigationService.NavigateAsync(typeof(MediaPlayer), diskItem);
        }

        public virtual async Task AddToPlaylist(DiskItem diskItem)
        {
            if (diskItem == null)
            {
                return;
            }

            var dialog = new PlaylistDialog(diskItem);
            await dialog.ShowAsync();
            if (dialog.Result.Count == 0)
            {
                return;
            }

            foreach (var playlist in dialog.Result)
            {
                await DataProvider.Instance.AddToPlayListAsync(playlist, diskItem);
            }
        }

        public virtual async Task RemoveFromPlaylist(DiskItem diskItem)
        {
            var dialog = new PlaylistDialog(diskItem, 1);
            await dialog.ShowAsync();
            if (dialog.Result.Count == 0)
            {
                return;
            }

            foreach (var playlist in dialog.Result)
            {
                await DataProvider.Instance.RemoveFromPlayListAsync(playlist, diskItem);
            }
        }

        public virtual async Task SelectAndUpload(CancellationTokenSource source, List<string> extensions = null)
        {
            var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            (extensions == null || extensions.Count == 0 ? FileTypes.Extensions.Keys.ToList() : extensions).ForEach(e => picker.FileTypeFilter.Add(e));
            var files = await picker.PickMultipleFilesAsync();
            if (files == null)
            {
                return;
            }

            await SelectDiskFolderAndUpload(files, source);
        }

        public virtual async Task SelectDiskFolderAndUpload(IReadOnlyList<StorageFile> files, CancellationTokenSource source)
        {
            var dialog = new BrowseDialog();
            await dialog.ShowAsync();
            if (string.IsNullOrEmpty(dialog.SelectedFolder))
            {
                return;
            }

            if (source.IsCancellationRequested)
            {
                source = new CancellationTokenSource();
            }

            var resultItems = new List<DiskItem>();
            foreach (var file in files)
            {
                var disiredPath = $"{dialog.SelectedFolder.TrimEnd('/')}/{file.Name}";
                var uploadURL = await DiskService.Client.GetUploadURL(disiredPath, true);
                var result = await DiskService.Client.UploadFile(uploadURL, file, source.Token);
                if (!result.IsError())
                {
                    var newItem = new DiskItem()
                    {
                        DisplayName = $"{file.DisplayName}{file.FileType}",
                        Path = disiredPath,
                        IsFolder = false
                    };
                    resultItems.Add(newItem);
                    newItem = await DataProvider.Instance.CreateItemAsync(newItem);
                }
            }
            if (resultItems.Count == 0)
            {
                return;
            }

            if (SettingsService.Instance.NavigateAfterUpload)
            {
                var qdialog = new ContentDialog()
                {
                    Title = "Go to upload folder?",
                    Content = $"File(s) uploaded successfully. Do you want to navigate to '{resultItems.FirstOrDefault().ParentFolder}' directory?",
                    CloseButtonText = "No",
                    PrimaryButtonText = "Yes"
                };
                var dialogResult = await qdialog.ShowAsync();
                if (dialogResult == ContentDialogResult.Primary)
                {
                    await NavigationService.NavigateAsync(typeof(MainPage), resultItems.FirstOrDefault().ParentFolder);
                    return;
                }
            }

            if (Items.Path == dialog.SelectedFolder)
            {
                resultItems.ForEach(i => Items.Add(i));
            }
        }
    }
}
