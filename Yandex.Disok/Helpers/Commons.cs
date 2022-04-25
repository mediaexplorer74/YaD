using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Ya.D.Helpers
{
    public class Commons
    {
        private static object locker = new object();
        private static Lazy<Commons> _instance = new Lazy<Commons>(() => new Commons());

        private readonly Dictionary<Uri, byte[]> _imagesCache = new Dictionary<Uri, byte[]>();

        private Commons() { }

        public static Commons Instance => _instance.Value;

        public int ImageSize = 450;
        public const string DBNAME = "main.db";

        // * DIY ZONE *
        
        public const string YAD_CLIENT_ID 
            = "26c6c3fab46f4c2ead789ad85dbf07e2"; // "CLIENT_ID"

        public const string YAD_URL =
            "https://oauth.yandex.ru/authorize?response_type=token&client_id=" + YAD_CLIENT_ID;

#if DEBUG
        public const string FEEDBACK_URL = "https://localhost/api/v1/feedbacks";
#else
        public const string FEEDBACK_URL = "https://localhost/api/v1/feedbacks";        
#endif

        public Dictionary<string, string> BaseHeaders { get; set; } = new Dictionary<string, string>() { { "Host", "webdav.yandex.ru" }, { "Accept", "*/*" } };

        public byte[] GetIcon(Uri sourceUri)
        {
            return GetIconAsync(sourceUri).Result;
        }

        public async Task<byte[]> GetIconAsync(Uri sourceUri)
        {
            if (_imagesCache.ContainsKey(sourceUri))
                return _imagesCache[sourceUri];

            var file = await StorageFile.GetFileFromApplicationUriAsync(sourceUri);
            using (var inputStream = await file.OpenSequentialReadAsync())
            {
                var readStream = inputStream.AsStreamForRead();
                var buffer = new byte[readStream.Length];
                await readStream.ReadAsync(buffer, 0, buffer.Length);
                if (!_imagesCache.ContainsKey(sourceUri))
                    _imagesCache.Add(sourceUri, buffer);
                return buffer;
            }
        }
    }
}
