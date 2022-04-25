using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ya.D.Models
{
    public class DiskInfo : DiskBaseModel
    {
        private long _total = 0;
        private long _used = 0;

        [JsonProperty("trash_size")]
        public long TrashSize { get; set; }
        [JsonProperty("total_space")]
        public long TotalSpace { get { return _total; } set { Set(ref _total, value); AvailableSpace = _total - _used; } }
        [JsonProperty("used_space")]
        public long UsedSpace { get { return _used; } set { Set(ref _used, value); AvailableSpace = _total - _used; } }
        [JsonProperty("system_folders")]
        public Dictionary<string, string> SystemFolders { get; set; } = new Dictionary<string, string>();

        public long AvailableSpace { get; set; }
    }
}
