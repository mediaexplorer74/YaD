using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;
using Ya.D.Helpers;
using Ya.D.Models;
using Ya.D.Services;
using Ya.D.Services.SettingsServices;
using Ya.D.Views;
using Ya.D.Views.Dialogs;

namespace Ya.D.ViewModels
{
    public class MainPageViewModel : BaseItemsViewModel
    {
        private bool _canLoadPreviews = SettingsService.Instance.LoadPreviews && (SettingsService.Instance.GotInetConnection && SettingsService.Instance.LoadWithGoodInet || !SettingsService.Instance.LoadWithGoodInet);
        private bool _selectFolder;
        private bool _selectFiles;

        private CancellationTokenSource _source = new CancellationTokenSource();
        private BrowseSettings _settings = new BrowseSettings();
        private ObservableCollection<CrumbItem> _crumbs = new ObservableCollection<CrumbItem>();

        public bool LoadPreviews { get => _canLoadPreviews; set => Set(ref _canLoadPreviews, value); }
        public bool CanSelectFolder { get => _selectFolder; set => Set(ref _selectFolder, value); }
        public bool CanSelectFiles { get => _selectFiles; set => Set(ref _selectFiles, value); }
        public BrowseSettings PageSettings { get => _settings; set => Set(ref _settings, value); }
        public ObservableCollection<CrumbItem> Crumbs { get => _crumbs; set => Set(ref _crumbs, value); }

        public DelegateCommand<string> LoadItemsCMD { get; }

        public MainPageViewModel()
        {
            LoadItemsCMD = new DelegateCommand<string>(async path => { await NavigationService.NavigateAsync(typeof(MainPage), path); });
            DownloadCMD = new DelegateCommand<DiskItem>(async item => await DownloadItem(item));
            DeleteCMD = new DelegateCommand<DiskItem>(async item => await DeleteItem(item));
        }

        #region Page navigation

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            SelectedItem = null;
            if (suspensionState != null && suspensionState.ContainsKey(nameof(PageSettings)))
            {
                PageSettings = suspensionState[nameof(PageSettings)] as BrowseSettings;
            }
            else
            {
                switch (parameter)
                {
                    case BrowseSettings settings:
                        PageSettings = settings;
                        break;
                    case string path:
                        PageSettings.SearchPath = path;
                        break;
                }
            }

            Items = new DiskItemIncrementalLoading() { Path = PageSettings.SearchPath };
            UpdateCrumbs();
            await Items.LoadMoreItemsAsync(1);
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                if (!suspensionState.ContainsKey(nameof(PageSettings)))
                {
                    suspensionState.Add(nameof(PageSettings), PageSettings);
                }

                suspensionState[nameof(PageSettings)] = PageSettings;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }

        #endregion

        public void GotoSettings() =>
            Shell.NavigationService.Navigate(typeof(SettingsPage), 0);

        public void GotoAbout() =>
            Shell.NavigationService.Navigate(typeof(SettingsPage), 1);

        public void GotoPlayer() =>
            Shell.NavigationService.Navigate(typeof(MediaPlayer));

        #region Commands for items

        public async Task Upload()
        {
            var picker = new FileOpenPicker { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            FileTypes.Extensions.Select(i => i.Key).ToList().ForEach(e => picker.FileTypeFilter.Add(e));
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

        public async Task UploadFolder()
        {
            var dialog = new FolderPicker();
            dialog.ViewMode = PickerViewMode.Thumbnail;
            dialog.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            FileTypes.Extensions.Keys.ToList().ForEach(e => dialog.FileTypeFilter.Add(e));
            var folder = await dialog.PickSingleFolderAsync();
            if (folder == null)
            {
                return;
            }

            await RecursionUpload(folder, PageSettings.SearchPath, true);
        }

        public async Task CreateFolder()
        {
            var dialog = new InputDialog();
            await dialog.ShowAsync();
            if (string.IsNullOrWhiteSpace(dialog.Result))
            {
                return;
            }

            var newFolderPath = $"{PageSettings.SearchPath.TrimEnd('/')}/{WebUtility.UrlEncode(dialog.Result)}/";
            var result = await DiskService.Client.CreateFolder(newFolderPath);
            if (result.IsError())
            {
                return;
            }

            var newItem = new DiskItem
            {
                IsFolder = true,
                Path = newFolderPath,
                DisplayName = dialog.Result
            };
            newItem = await DataProvider.Instance.CreateItemAsync(newItem);
            Items.Add(newItem);
        }

        private async Task RecursionUpload(StorageFolder storageFolder, string rootPath, bool isRoot = false)
        {
            // add current folder
            var newFolderPath = $"{rootPath.TrimEnd('/')}/{storageFolder.DisplayName}/";
            var uploadResult = await DiskService.Client.CreateFolder(newFolderPath);
            if (uploadResult.IsError())
            {
                return;
            }

            if (isRoot)
            {
                var newItem = await AddNewItem(newFolderPath, storageFolder.DisplayName, true);
                Items.Add(newItem);
            }

            // add inner folders
            foreach (var directory in await storageFolder.GetFoldersAsync())
            {
                await RecursionUpload(directory, newFolderPath);
            }

            // add files
            var files = await storageFolder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByName);
            foreach (var file in files)
            {
                var disiredPath = $"{newFolderPath.TrimEnd('/')}/{file.DisplayName}{file.FileType}";
                var uploadURL = await DiskService.Client.GetUploadURL(disiredPath, true);
                var result = await DiskService.Client.UploadFile(uploadURL, file, new CancellationTokenSource().Token);
                if (!result.IsError())
                {
                    await AddNewItem($"{file.DisplayName}{file.FileType}", disiredPath);
                }
            }
        }

        private Task<DiskItem> AddNewItem(string path, string displayName, bool isDir = false)
        {
            var newItem = new DiskItem() { DisplayName = displayName, Path = path, ItemType = isDir ? "dir" : "file" };
            return DataProvider.Instance.UpdateItemAsync(newItem);
        }

        #endregion

        #region Load items

        public void UpdateCrumbs()
        {
            #region Fill crumbs data

            if (!PageSettings.SelectFolder)
            {
                SelectedItem = null;
            }

            Crumbs.Clear();
            Crumbs.Add(new CrumbItem() { DisplayPath = "disk", FolderPath = "/" });
            var crumbItems = string.IsNullOrEmpty(PageSettings.SearchPath?.Trim('/')) ? new string[0] : $"{PageSettings.SearchPath.Trim('/')}".Split('/');
            for (var i = 0; i < crumbItems.Length; i++)
            {
                Crumbs.Add(new CrumbItem()
                {
                    DisplayPath = WebUtility.UrlDecode(crumbItems[i]),
                    FolderPath = string.Join("/", crumbItems.Take(i + 1))
                });
            }
            #endregion
        }

        public override async Task ItemChanged()
        {
            if (SelectedItem == null)
            {
                return;
            }

            if (SelectedItem.IsFolder)
            {
                await Shell.NavigationService.NavigateAsync(typeof(MainPage), SelectedItem.Path);
            }
            else if (!CanSelectFolder && !CanSelectFiles)
            {
                await Shell.NavigationService.NavigateAsync(typeof(DetailPage), new DetailsData { ItemToSelect = SelectedItem, Items = Items.ToList() });
            }
        }

        #endregion
    }
}
