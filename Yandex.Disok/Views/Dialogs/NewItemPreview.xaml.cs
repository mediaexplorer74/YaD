using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Ya.D.Services;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ya.D.Views.Dialogs
{
    public sealed partial class NewItemPreview : ContentDialog
    {
        public bool DialogResult { get; set; } = false;

        public NewItemPreview(StorageFile file, bool isVideo = false)
        {
            InitializeComponent();
            var tmpFile = file;
            Task.Factory.StartNew(async () =>
            {
                if (!isVideo)
                {
                    using (IRandomAccessStream stream = await tmpFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                        SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                        SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                        SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
                        await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);
                        ImagePreview.Source = bitmapSource;
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.None, DiskService.Client.Sync);

        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            DialogResult = true;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
