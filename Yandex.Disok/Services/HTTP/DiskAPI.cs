using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Ya.D.Models;
using Ya.D.Services.SettingsServices;

namespace Ya.D.Services.HTTP
{
    public class DiskAPI
    {
        private readonly string Token;
        public TaskScheduler Sync { get; }

        public const string APIURL = "https://cloud-api.yandex.net/v1/disk/";
        public const string FLATSEARCH = "name,public_url,created,modified,preview,mime_type,type,size";

        public DiskInfo Info { get; private set; }

        public DiskAPI()
        {
            Sync = TaskScheduler.FromCurrentSynchronizationContext();
            // Never mind...
            Token = SettingsService.Instance.CurrentUserData.Token;
            Task.Run(async () => Info = await GetDiskInfo());
        }

        public async Task<DiskInfo> GetDiskInfo()
        {
            Info = await GetAsync<DiskInfo>();
            return Info;
        }

        public async Task<DiskOperationStatus> GetOperationStatus(string operationID)
            => await GetAsync<DiskOperationStatus>($"operations/{operationID}");

        public async Task<DiskResponse> GetItemInfo(string itemPath, int previewSize = 120)
            => await GetAsync<DiskResponse>($"resources?path={itemPath}&preview_size={previewSize}x");

        public async Task<List<DiskItem>> GetDiskItemsAsync(string path, int limit = 50, int offset = 0, int previewWidth = 120, bool crop = false)
            => (await GetAsync<DiskResponse>($"resources?path={path}&preview_size={previewWidth}x&offset={offset}&limit={limit}&preview_crop={crop}", useCache: true)).Embedded.Items;

        public async Task<List<DiskItem>> GetDiskItemsFlatAsync(int limit = 0, int offset = 0, bool crop = false, DiskMediaType media = DiskMediaType.audio | DiskMediaType.video)
            => (await GetAsync<Embedded>($"resources/files?fields={FLATSEARCH}&preview_size=120x&preview_crop={crop}&media_type={GetMediaType(media)}{(limit == 0 ? string.Empty : $"&limit={limit}")}{(offset == 0 ? string.Empty : $"&offset={offset}")}", useCache: true)).Items;

        public async Task<List<DiskItem>> GetLastUploaded(int limit = 20, bool crop = false, DiskMediaType media = DiskMediaType.audio | DiskMediaType.video)
            => (await GetAsync<Embedded>($"last-uploaded?preview_size=120x&preview_crop={crop}&media_type={GetMediaType(media)}&fields={FLATSEARCH}{(limit == 0 ? string.Empty : $"&limit={limit}")}")).Items;

        public async Task<DiskURL> GetUploadURL(string itemPath, bool overwrite = false)
            => await GetAsync<DiskURL>($"resources/upload?path={itemPath}&overwrite={overwrite}");

        public async Task<DiskURL> UploadFromLink(string url, string folderPath, bool withRedirects = false)
            => await GetAsync<DiskURL>($"resources/upload?url={url}&path={folderPath}&disable_redirects={withRedirects}");

        public async Task<DiskURL> GetDownloadURL(string itemPath)
            => await GetAsync<DiskURL>($"resources/download?path={itemPath}&fields={FLATSEARCH}");

        public async Task<DiskURL> Copy(string from, string to, bool overwrite = false)
            => await GetAsync<DiskURL>($"resources/copy?from={from}&to={to}&overwrite={overwrite}&fields={FLATSEARCH}", method: "POST");

        public async Task<DiskURL> Move(string from, string to, bool overwrite = false)
            => await GetAsync<DiskURL>($"resources/move?from={from}&to={to}&overwrite={overwrite}&fields={FLATSEARCH}", method: "POST");

        public async Task<DiskURL> Delete(string path, bool permanently = false)
             => await GetAsync<DiskURL>($"resources?path={path}&permanently={permanently}&fields={FLATSEARCH}", method: "DELETE");

        public async Task<DiskURL> CreateFolder(string path)
            => await GetAsync<DiskURL>($"resources?path={path}&fields={FLATSEARCH}", method: "PUT");

        public async Task<DiskURL> Publish(string path, bool publish = true)
            => await GetAsync<DiskURL>($"resources/{(publish ? "publish" : "unpublish")}?path={path}", method: "PUT");

        public async Task<DiskURL> DeleteFromTrash(string path)
            => await GetAsync<DiskURL>($"trash/resources?path={path}", method: "DELETE");

        public async Task<DiskURL> RestoreFromTrash(string path, string newName = "", bool overwrite = false)
            => await GetAsync<DiskURL>($"trash/resources/restore?path={path}&overwrite={overwrite}{(string.IsNullOrWhiteSpace(newName) ? string.Empty : $"&name={newName}")}", method: "PUT");

        public async Task<DiskBaseModel> UploadFile(DiskURL diskURL, IStorageFile file, CancellationToken token, Action<long, long> progressAction = null)
        {
            var result = new DiskBaseModel();
            var uploader = new BackgroundUploader() { Method = "PUT" };
            uploader.SetRequestHeader("Authorization", $"OAuth {Token}");
            uploader.SetRequestHeader("Accept", $"application/json");
            var upload = uploader.CreateUpload(new Uri(diskURL.URL), file);
            await HandleUploadAsync(upload, progressAction, true, token == null ? new CancellationTokenSource().Token : token);
            return result;
        }

        public async Task<DiskBaseModel> DownLoadFileAsync(DiskURL diskUrl, IStorageFolder folder, string fileName, CancellationToken token, Action<long, long> progressAction = null)
        {
            var result = new DiskBaseModel();
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            var downloader = new BackgroundUploader() { Method = "PUT" };
            downloader.SetRequestHeader("Authorization", $"OAuth {Token}");
            downloader.SetRequestHeader("Accept", $"application/json");
            var download = downloader.CreateUpload(new Uri(diskUrl.URL), file);
            await HandleUploadAsync(download, progressAction, true, token == null ? new CancellationTokenSource().Token : token);
            return result;
        }

        public async Task<byte[]> GetBinary(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            using (var client = PrepareClient())
            {
                var response = await client.GetAsync(url);
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        public async Task<byte[]> GetPreview(string itemPath, int previewSize = 120)
        {
            var data = await GetItemInfo(itemPath, previewSize);
            if (data == null || data.IsError())
            {
                return null;
            }

            return await GetBinary(data.PreviewURL);
        }

        #region Util methond

        private async Task<DiskBaseModel> GetAsync(string path = "", Dictionary<string, string> headers = null, string method = "GET")
        {
            var result = new DiskBaseModel();
            Debug.WriteLine($"Try get {path}");
            using (var client = PrepareClient())
            {
                var data = string.Empty;
                HttpResponseMessage response = null;
                try
                {
                    switch (method)
                    {
                        case "GET":
                            response = await client.GetAsync(GetURI(path));
                            break;
                        case "POST":
                            response = await client.PostAsync(GetURI(path), null);
                            break;
                        case "PATCH":
                            var message = new HttpRequestMessage(new HttpMethod("PATCH"), GetURI(path));
                            response = await client.SendAsync(message);
                            break;
                        case "PUT":
                            response = await client.PutAsync(GetURI(path), null);
                            break;
                        case "DELETE":
                            response = await client.DeleteAsync(GetURI(path));
                            break;
                    }
                }
                catch (Exception ex)
                {
                    return ErrorHandler.Instance.Handle(ex, ErrorType.HTTPRequest, method);
                }

                try
                {
                    data = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    return ErrorHandler.Instance.Handle(ex, ErrorType.HTTPResponse, method);
                }

                if (!response.IsSuccessStatusCode)
                {
                    result = JsonConvert.DeserializeObject<DiskBaseModel>(data);
                }
                result.Code = (int)response.StatusCode;
                result.PureResponse = data;
                Debug.WriteLine($"Response: {response.StatusCode} (Success: {response.IsSuccessStatusCode}), {result.PureResponse.Length} bytes");
            }
            return result;
        }

        private async Task<T> GetAsync<T>(string path = "", Dictionary<string, string> headers = null, string method = "GET", bool useCache = false) where T : class, new()
        {
            if (useCache)
            {
                var cachedData = CachingService.Cache.GetItem<T>(path);
                if (cachedData != null)
                {
                    return cachedData;
                }
            }
            var result = new T();
            try
            {
                var data = await GetAsync(path, headers, method);
                if (data.IsError() && result is IEnumerable list)
                {
                    return result;
                }
                result = JsonConvert.DeserializeObject<T>(data.PureResponse);
                if (useCache)
                {
                    CachingService.Cache.AddItem(path, result);
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Instance.Handle(ex, ErrorType.Serialization);
            }

            return result;
        }

        private HttpClient PrepareClient(string path = "", Dictionary<string, string> headers = null)
        {
            var result = new HttpClient();
            result.DefaultRequestHeaders.Add("Authorization", $"OAuth {Token}");
            result.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    if (!result.DefaultRequestHeaders.Contains(header.Key))
                    {
                        result.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
            }
            return result;
        }

        private Uri GetURI(string path = "")
        {
            return new Uri($"{APIURL}{path?.Trim('/')}");
        }

        private string GetMediaType(DiskMediaType type)
        {
            var result = new List<DiskMediaType>();
            if (type.HasFlag(DiskMediaType.all))
            {
                return string.Join(",", Enum.GetNames(typeof(DiskMediaType))).Replace("all,", string.Empty);
            }

            foreach (var item in Enum.GetValues(typeof(DiskMediaType)))
            {
                if (type.HasFlag((DiskMediaType)item))
                {
                    result.Add((DiskMediaType)item);
                }
            }
            return string.Join(",", result);
        }

        #endregion

        #region Upload Download handlers

        private async Task HandleUploadAsync(UploadOperation upload, Action<long, long> progress, bool start, CancellationToken cancelToken)
        {
            try
            {
                Progress<UploadOperation> progressCallback = new Progress<UploadOperation>(uploadOperation =>
                {
                    if (uploadOperation.Progress.TotalBytesToSend > 0)
                    {
                        if (progress != null)
                        {
                            progress.Invoke((long)(uploadOperation.Progress.BytesSent / 1024), (long)(uploadOperation.Progress.TotalBytesToSend / 1024));
                        }
                    }
                });
                if (start)
                {
                    await upload.StartAsync().AsTask(cancelToken, progressCallback);
                }
                else
                {
                    await upload.AttachAsync().AsTask(cancelToken, progressCallback);
                }
            }
            catch (Exception)
            {
                // throw ExceptionHandler.Handle(ex);
            }
        }

        private async Task HandleDownloadAsync(DownloadOperation download, Action<long, long> progress, bool start, CancellationToken cancelToken)
        {
            try
            {
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(downloadOperation =>
                {
                    if (downloadOperation.Progress.TotalBytesToReceive > 0)
                    {
                        if (progress != null)
                        {
                            progress.Invoke((long)(downloadOperation.Progress.BytesReceived / 1024), (long)(downloadOperation.Progress.TotalBytesToReceive / 1024));
                        }
                    }
                });
                if (start)
                {
                    await download.StartAsync().AsTask(cancelToken, progressCallback);
                }
                else
                {
                    await download.AttachAsync().AsTask(cancelToken, progressCallback);
                }
            }
            catch (Exception)
            {
                // throw ExceptionHandler.Handle(ex);
            }
        }

        #endregion
    }
}
