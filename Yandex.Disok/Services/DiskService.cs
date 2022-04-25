using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Ya.D.Helpers;
using Ya.D.Models;
using Ya.D.Services.HTTP;

namespace Ya.D.Services
{
    public class DiskService
    {
        private static readonly Lazy<DiskService> _instance = new Lazy<DiskService>(() => new DiskService());
        private static readonly Lazy<DiskAPI> _client = new Lazy<DiskAPI>(() => new DiskAPI());

        public static DiskService Instance => _instance.Value;
        public static DiskAPI Client => _client.Value;

        private DiskService()
        {
        }

        public async Task<bool> DownloadItem(DiskItem diskItem)
        {
            if (diskItem == null)
            {
                return false;
            }

            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            if (diskItem.DisplayName.Contains("."))
            {
                picker.FileTypeFilter.Add($".{diskItem.DisplayName.Split('.').LastOrDefault()}");
            }
            else
            {
                FileTypes.Extensions.Keys.ToList().ForEach(e => picker.FileTypeFilter.Add(e));
            }

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                if (!diskItem.IsFolder)
                {
                    var downloadUrl = await Client.GetDownloadURL(diskItem.Path);
                    if (downloadUrl.IsError())
                    {
                        return false;
                    }

                    var result = await Client.DownLoadFileAsync(downloadUrl, folder, diskItem.DisplayName, new CancellationTokenSource().Token);
                    return !result.IsError();
                }
            }
            return false;
        }

        public async Task<bool> DeleteItem(DiskItem diskItem, Action<DiskItem> action = null)
        {
            if (diskItem == null)
            {
                return false;
            }

            var dialog = new ContentDialog()
            {
                Title = "Delete disk item?",
                Content = $"Are you sure you want to delete '{diskItem.DisplayName}'?",
                CloseButtonText = "No",
                PrimaryButtonText = "Delete"
            };
            var dialogResult = await dialog.ShowAsync();
            if (dialogResult != ContentDialogResult.Primary)
            {
                return false;
            }

            var result = await Client.Delete(diskItem.Path);
            if (result != null && result.IsError())
            {
                return false;
            }

            if (action != null)
            {
                await Task.Factory.StartNew(() => action(diskItem), CancellationToken.None, TaskCreationOptions.None, Client.Sync);
            }

            return true;
        }
    }
}
