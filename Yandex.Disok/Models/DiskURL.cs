using Newtonsoft.Json;

namespace Ya.D.Models
{
    public class DiskURL : DiskBaseModel
    {
        public bool Templated { get; set; }
        [JsonProperty("href")]
        public string URL { get; set; }
        public string Method { get; set; }
    }
}
