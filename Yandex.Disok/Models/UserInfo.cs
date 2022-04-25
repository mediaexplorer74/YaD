using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ya.D.Models
{
    public class UserInfo : DiskBaseModel
    {
        private bool _loggedOut;
        private bool _avatarEmpty = true;
        private string _displayName = string.Empty;
        private string _avatarID = string.Empty;

        public string TokeType { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; } = DateTime.MinValue;

        public bool LoggedOut { get => _loggedOut; set => Set(ref _loggedOut, value); }
        [JsonProperty("is_avatar_empty")]
        public bool IsAvatarEmpty { get => _avatarEmpty; set { _avatarEmpty = value; SetAvatarURI(); } }
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        [JsonProperty("display_name")]
        public string DisplayName { get => _displayName; set { _displayName = value; FillPieces(); } }
        [JsonProperty("real_name")]
        public string RealName { get; set; }
        [JsonProperty("default_avatar_id")]
        public string AvatarID { get => _avatarID; set { _avatarID = value; SetAvatarURI(); } }
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("sex")]
        public string Sex { get; set; }
        [JsonProperty("default_email")]
        public string DefaultEmail { get; set; }
        [JsonProperty("birthday")]
        public DateTime? Birthday { get; set; }
        [JsonProperty("emails")]
        public List<string> Emails { get; set; } = new List<string>();

        [JsonIgnore]
        public Uri ImagePath { get; set; }
        [JsonIgnore]
        public List<NamePiece> NamePieces { get; set; } = new List<NamePiece>();

        public bool CanUse()
        {
            if (string.IsNullOrWhiteSpace(Token))
                Error = "No auth token";
            //else if (DateTime.Now > ExpirationDate)
            //    Error = "Token expired";
            //else if (string.IsNullOrWhiteSpace(TokeType))
            //    Error = "Unknown token type";
            return !IsError();
        }

        private void SetAvatarURI()
        {
            if (IsAvatarEmpty || AvatarID.StartsWith("0/"))
                ImagePath = new Uri("ms-appx:///Assets/Icons/icon_profile.png");
            else
                ImagePath = new Uri($"https://avatars.yandex.net/get-yapic/{AvatarID}/islands-200");
        }

        private void FillPieces()
        {
            if (string.IsNullOrEmpty(DisplayName))
                return;
            var words = DisplayName.Split(' ');

            foreach (var word in words)
            {
                if (word.Length > 1)
                    NamePieces.Add(new NamePiece { First = word.Substring(0, 1).ToUpperInvariant(), Rest = word.Length == 1 ? string.Empty : word.Substring(1, word.Length - 1) });
            }
        }
    }

    public class NamePiece : DiskBaseModel
    {
        public string First { get; set; }
        public string Rest { get; set; }
    }
}
