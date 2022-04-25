using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Navigation;
using Ya.D.Helpers;
using Ya.D.Models;
using Ya.D.Services;

namespace Ya.D.ViewModels
{
    public class DetailPageViewModel : BaseItemsViewModel
    {
        private bool _isPlayable;
        private bool _shared;
        private string _location;
        private List<DiskItem> _items;

        public bool IsPlayable 
        { 
            get => _isPlayable; 
            set => Set(ref _isPlayable, value); 
        }

        public bool Shared 
        { 
            get => _shared; 
            set 
            { if (Set(ref _shared, value)) { ChangeSharedState(); } 
            } 
        }

        public string Location 
        { 
            get => _location; 
            set => Set(ref _location, value); 
        }

        public List<DiskItem> DiskItems 
        { 
            get => _items; 
            set => Set(ref _items, value); 
        }

        public DelegateCommand CopyLinkCMD { get; }
        public DelegateCommand PlayCMD { get; }

        // DetailPageViewModel
        public DetailPageViewModel()
        {
            PlayCMD = new DelegateCommand(async () => await TryToPlayAsync(SelectedItem));
            DownloadCMD = new DelegateCommand<DiskItem>(async _ => await DownloadItem(SelectedItem));
            DeleteCMD = new DelegateCommand<DiskItem>(async _ => await DeleteItem(SelectedItem));
            CopyLinkCMD = new DelegateCommand(() => WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                if (string.IsNullOrWhiteSpace(SelectedItem?.PublicURL))
                {
                    {
                        return;
                    }
                }

                var dataPackage = new DataPackage { RequestedOperation = DataPackageOperation.Copy };
                dataPackage.SetText(SelectedItem.PublicURL);
                Clipboard.SetContent(dataPackage);
            }), () => !string.IsNullOrWhiteSpace(SelectedItem?.PublicURL ?? string.Empty));

        }//DetailPageViewModel end


        // OnNavigatedToAsync
        public override async Task OnNavigatedToAsync
        (
            object parameter, NavigationMode mode, 
            IDictionary<string, object> suspensionState
        )
        {
            if (parameter is DetailsData details)
            {
                DiskItems = details.Items;
                SelectedItem = details.ItemToSelect;
            }

            await Task.CompletedTask;

        }//OnNavigatedToAsync end 


        // OnNavigatedFromAsync
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(SelectedItem)] = JsonConvert.SerializeObject(SelectedItem);
            }

            await Task.CompletedTask;

        }//OnNavigatedFromAsync end 


        // OnNavigatingFromAsync
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;

            await Task.CompletedTask;

        }//OnNavigatingFromAsync end


        // GoToDirectory
        public void GoToDirectory()
        {
            NavigationService.Navigate(typeof(Views.MainPage), SelectedItem.ParentFolder);
        }//GoToDirectory end


        // ChangeSharedState
        private void ChangeSharedState()
        {
            Task.Factory.StartNew
            (
                async () => await ChangeSharedStateAsync(), 
                CancellationToken.None, 
                TaskCreationOptions.None, 
                DiskService.Client.Sync
            );

        }//ChangeSharedState end


        // ChangeSharedStateAsync
        private async Task ChangeSharedStateAsync()
        {
            DiskURL diskURL;
            if 
            (
                string.IsNullOrEmpty(SelectedItem.PublicURL) && Shared 
                ||
                !string.IsNullOrEmpty(SelectedItem.PublicURL) && !Shared
            )
            {
                diskURL = await DiskService.Client.Publish(SelectedItem.Path, Shared);
                if (!diskURL.IsError())
                {
                    var result = !Shared ? string.Empty : (await DiskService.Client.GetItemInfo(SelectedItem.Path)).PublicURL;
                    SelectedItem.PublicURL = result;
                }
            }

        }//ChangeSharedStateAsync end


        // ItemChanged
        public override async Task ItemChanged()
        {
            if (SelectedItem != null)
            {
                Shared = !string.IsNullOrWhiteSpace(SelectedItem.PublicURL);
                if (SelectedItem.BigPreviewImage == null)
                {
                    var image = await DiskService.Client.GetPreview(SelectedItem.Path, Commons.Instance.ImageSize);
                    SelectedItem.BigPreviewImage = image;
                }
                if (SelectedItem != null)
                {
                    IsPlayable = SelectedItem.MimeType.Contains("audio") || SelectedItem.MimeType.Contains("video");
                }
            }

        }//ItemChanged end
    }
}
