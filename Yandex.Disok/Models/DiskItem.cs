using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Ya.D.Helpers;

namespace Ya.D.Models
{
    public class DiskResponse : DiskBaseModel
    {
        [JsonProperty("preview")]
        public string PreviewURL { get; set; }
        [JsonProperty("public_key")]
        public string PublicKey { get; set; }
        [JsonProperty("public_url")]
        public string PublicURL { get; set; }
        public string Path { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("created")]
        public DateTime Created { get; set; }
        [JsonProperty("modified")]
        public DateTime Modified { get; set; }
        [JsonProperty("_embedded")]
        public Embedded Embedded { get; set; } = new Embedded();
    }

    public class Embedded : DiskBaseModel
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Sort { get; set; }
        public string Path { get; set; }
        public List<DiskItem> Items { get; set; } = new List<DiskItem>();
    }

    public class DiskItem : DiskBaseModel
    {
        private string _itemtype = "dir";
        private string _path = string.Empty;
        private string _publicUrl = string.Empty;
        private string _parentFolder = string.Empty;
        private byte[] _image = null;
        private byte[] _bigImage = null;
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint ID { get; set; }
        [JsonProperty("type")]
        public string ItemType { get { return _itemtype; } set { _itemtype = value; IsFolder = value == "dir"; } }
        [JsonIgnore]
        public bool IsFolder { get; set; }
        [JsonProperty("size")]
        public long ContentLength { get; set; } = 0;
        [JsonProperty("preview")]
        public string PreviewURL { get; set; }
        [JsonProperty("public_url")]
        public string PublicURL { get { return _publicUrl; } set { Set(ref _publicUrl, value); } }
        [JsonProperty("path")]
        public string Path { get { return _path; } set { Set(ref _path, value?.Replace("disk:", string.Empty)); ParentFolder = DataUtils.GetParentFolderPath(value); } }
        public string ParentFolder { get { return _parentFolder; } set { Set(ref _parentFolder, value); } }
        [JsonProperty("name")]
        public string DisplayName { get; set; }
        [JsonProperty("mime_type")]
        public string MimeType { get; set; }
        [JsonProperty("created")]
        public DateTime Created { get; set; } = DateTime.Now;
        [JsonProperty("modified")]
        public DateTime Modified { get; set; }

        [JsonIgnore]
        public byte[] PreviewImage { get { return _image; } set { Set(ref _image, value); } }
        [JsonIgnore]
        public byte[] BigPreviewImage { get { return _bigImage; } set { Set(ref _bigImage, value); } }

        public virtual List<ItemList> PlayLists { get; set; }

        public DiskItem()
        {
            PlayLists = new List<ItemList>();
        }

        public override bool Equals(object obj)
        {
            var other = obj as DiskItem;
            if (other == null || string.IsNullOrWhiteSpace(Path) || string.IsNullOrWhiteSpace(other.Path))
                return false;
            return Path.Equals(other.Path);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public void CopyFromOther(DiskItem item)
        {
            ItemType = item.ItemType;
            IsFolder = item.IsFolder;
            ContentLength = item.ContentLength;
            PreviewURL = item.PreviewURL;
            PublicURL = item.PublicURL;
            Path = item.Path;
            ParentFolder = item.ParentFolder;
            DisplayName = item.DisplayName;
            MimeType = item.MimeType;
            Created = item.Created;
            Modified = item.Modified;
            PreviewImage = item.PreviewImage;
            BigPreviewImage = item.BigPreviewImage;
        }
    }

    public class DiskItemComparer : IEqualityComparer<DiskItem>
    {
        public bool Equals(DiskItem x, DiskItem y)
        {
            return x?.Equals(y) ?? false;
        }

        public int GetHashCode(DiskItem obj)
        {
            return obj.GetHashCode();
        }
    }
}
