using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.Media.Capture;
using Windows.Storage;
using Ya.D.Helpers;
using Ya.D.Models;

namespace Ya.D.ViewModels
{
    public class PhotosViewModel : BaseItemsViewModel
    {
        public PhotosViewModel() : base()
        {
            Items = new DiskItemIncrementalLoading(DiskMediaType.image);
            GoToDetailsCMD = new DelegateCommand<DiskItem>(async i => await GoToDetails(i));
            DownloadCMD = new DelegateCommand<DiskItem>(async i => await DownloadItem(i));
            DeleteCMD = new DelegateCommand<DiskItem>(async i => await DeleteItem(i));
        }

        public async Task AddPhoto()
        {
            await SelectAndUpload(new CancellationTokenSource(), FileTypes.Extensions.Where(i => i.Key.StartsWith("image")).Select(i => i.Key).ToList());
        }

        public async Task TakeAPhoto()
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            var photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (photo == null)
            {
                return;
            }

            await SelectDiskFolderAndUpload(new List<StorageFile>() { photo }, new CancellationTokenSource());
            await photo.DeleteAsync();
        }

        public override async Task ItemChanged()
        {
            await GoToDetails(SelectedItem);
        }
    }
}