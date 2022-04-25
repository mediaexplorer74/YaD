using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Ya.D.Services.SettingsServices;

namespace Ya.D.Helpers
{
    public class DataUtils
    {
        public static string GetParentFolderPath(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "/";
            var names = fileName.Replace("disk:", string.Empty).Trim('/').Split('/');
            if (names.Length < 2)
                return "/";
            var result = new StringBuilder("/");
            for (int i = 0; i < names.Length - 1; i++)
            {
                result.Append($"{names[i]}/");
            }
            return result.ToString().TrimEnd('/');
        }

        public static Task<string> TryGetToken(Uri uri)
        {
            var data = uri.ToString();
            var regExp = new Regex("#[a-z_]+=([^\\&]+).*\\&[a-z_]+=(\\w+)\\&[a-z_]+=(\\d+)");
            if (regExp.IsMatch(data))
            {
                var matches = regExp.Matches(data);
                SettingsService.Instance.CurrentUserData = new Models.UserInfo()
                {
                    Token = matches[0].Groups[1].Value,
                    TokeType = matches[0].Groups[2].Value,
                    ExpirationDate = new DateTime(TimeSpan.FromSeconds(int.Parse(matches[0].Groups[3].Value)).Ticks)
                };
            }
            else
            {
                SettingsService.Instance.CurrentUserData = new Models.UserInfo();
            }
            return Task.FromResult(SettingsService.Instance.CurrentUserData.Token);
        }

        public static ImageSource SaveToImageSource(byte[] buffer)
        {
            if (buffer == null)
                return null;
            var result = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            using (DataWriter dw = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                dw.WriteBytes(buffer);
                dw.StoreAsync().GetResults();
                result.SetSource(stream);
            }
            return result;
        }
    }
}
