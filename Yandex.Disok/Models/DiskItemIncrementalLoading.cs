using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.UI.Xaml.Data;
using Windows.Foundation;
using Ya.D.Services;
using Ya.D.Helpers;
using Ya.D.Services.SettingsServices;

namespace Ya.D.Models
{
    public class DiskItemIncrementalLoading : ObservableCollection<DiskItem>, ISupportIncrementalLoading
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private bool _loading;
        private readonly bool _loadPreviews;
        private int _offset;
        private int _perPage = 25;
        private int _lastLoadCount = 25;
        private string _path;
        private readonly DiskMediaType _mediaType = DiskMediaType.all;
        private readonly Uri _defaultPreview;

        public bool IsLoading
        {
            get => _loading;
            set
            {
                Debug.WriteLine($"Loading: {(value ? "Yae" : "No")}");
                if (_loading == value) return;
                _loading = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsLoading)));
            }
        }

        public bool HasMoreItems { get => _lastLoadCount >= _perPage; }

        public string Path { get => _path; set => _path = value; }

        public CancellationTokenSource CancellationSource { get; set; }

        public DiskItemIncrementalLoading()
        {
            _loadPreviews = SettingsService.Instance.LoadPreviews;
            if (CancellationSource == null)
                CancellationSource = new CancellationTokenSource();
        }

        public DiskItemIncrementalLoading(DiskMediaType itemsType, Uri defaultUri = null) : this()
        {
            _mediaType = itemsType;
            _defaultPreview = defaultUri;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
            => AsyncInfo.Run((c) => LoadMoreItemsAsync(count, c));

        public async Task<LoadMoreItemsResult> LoadMoreItemsAsync(uint count, CancellationToken cancellationToken)
        {
            if (IsLoading)
                return new LoadMoreItemsResult { Count = 0 };
            IsLoading = true;
            _stopwatch.Restart();
            uint resultCount;
            List<DiskItem> data = new List<DiskItem>();
            List<DiskItem> actual = new List<DiskItem>();
            Func<DiskItem, bool> condition = null;
            Debug.WriteLine($"Start in {_stopwatch.ElapsedMilliseconds} ms.");
            if (_mediaType == DiskMediaType.all && !string.IsNullOrWhiteSpace(Path))
            {
                actual = await DiskService.Client.GetDiskItemsAsync(string.IsNullOrEmpty(_path) ? "/" : _path, _perPage, _offset * _perPage);
                condition = i => i.ParentFolder.Equals(_path);
            }
            else if (_mediaType != DiskMediaType.all)
            {
                var mime = _mediaType.ToString();
                actual = await DiskService.Client.GetDiskItemsFlatAsync(_perPage, _offset * _perPage, true, _mediaType);
                condition = i => !string.IsNullOrWhiteSpace(i.MimeType) && i.MimeType.StartsWith(mime);
            }
            if (_defaultPreview != null)
            {
                var image = await Commons.Instance.GetIconAsync(_defaultPreview);
                actual.ForEach(i => i.PreviewImage = image);
            }
            Debug.WriteLine($"Loaded in {_stopwatch.ElapsedMilliseconds} ms.");

            if (condition != null)
                data = await DataProvider.Instance.GetItemsAsync(condition, actual, _perPage, _offset * _perPage);
            if (data.Count > 0)
                _offset++;
            Debug.WriteLine($"Extracted from DB in {_stopwatch.ElapsedMilliseconds} ms.");
            _lastLoadCount = data.Count;
            data.ForEach(i => Add(i));
            UpdatePreviews(cancellationToken);
            resultCount = (uint)data.Count;
            IsLoading = false;
            _stopwatch.Stop();
            return new LoadMoreItemsResult { Count = resultCount };
        }

        private void UpdatePreviews(CancellationToken cts)
        {
            if (!_loadPreviews)
                return;
            Task.Factory.StartNew(async () =>
            {
                var toUpdate = new List<DiskItem>();
                var itemsTmp = new List<DiskItem>(Items);
                foreach (var item in itemsTmp)
                {
                    if (CancellationSource.IsCancellationRequested)
                        return;
                    if (item.IsFolder || !_loadPreviews || item.PreviewImage != null)
                        continue;
                    item.PreviewImage = string.IsNullOrWhiteSpace(item.PreviewURL) ?
                        await DiskService.Client.GetPreview(item.Path) :
                        await DiskService.Client.GetBinary(item.PreviewURL);
                    toUpdate.Add(item);
                }
                await DataProvider.Instance.UpdateItemsAsync(toUpdate);

            }, cts, TaskCreationOptions.None, DiskService.Client.Sync);

        }
    }
}
