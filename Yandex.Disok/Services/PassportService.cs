using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Ya.D.Helpers;
using Ya.D.Models;
using Ya.D.Services.HTTP;
using Ya.D.Services.SettingsServices;

namespace Ya.D.Services
{
    public class PassportService
    {
        private static readonly Lazy<PassportService> _instance = new Lazy<PassportService>(() => new PassportService());
        public static PassportService Instance => _instance.Value;

        public async Task<UserInfo> GetProfileData(string token = "")
        {
            var url = new Uri("https://login.yandex.ru/info?format=json");
            token = string.IsNullOrWhiteSpace(token) ? SettingsService.Instance.CurrentUserData.Token : token;
            try
            {
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("Authorization", $"OAuth {token}");
                    var response = await client.SendAsync(request);
                    var result = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(result);
                    userInfo.Token = token;
                    SettingsService.Instance.CurrentUserData = userInfo;
                    var exists = SettingsService.Instance.UsersData.FirstOrDefault(u => u.ID == userInfo.ID);
                    if (exists != null)
                    {
                        SettingsService.Instance.UsersData[SettingsService.Instance.UsersData.IndexOf(exists)] = userInfo;
                    }
                    else
                    {
                        SettingsService.Instance.UsersData.Add(userInfo);
                    }
                    return userInfo;
                }
            }
            catch (Exception)
            {
                return new UserInfo() { UserName = SettingsService.Instance?.CurrentUserData.UserName ?? string.Empty };
            }
        }

        public async Task<bool> DiscardToken()
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("access_token", SettingsService.Instance.CurrentUserData?.Token),
                new KeyValuePair<string, string>("client_id", Commons.YAD_CLIENT_ID)
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {SettingsService.Instance.CurrentUserData?.Token}");
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");
            try
            {
                var response = await client.PostAsync(new Uri("https://oauth.yandex.ru/"), content);
                var responseData = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                await FlourService.I.PushAnalytics("logout.error", ex.Message);
            }
            return true;
        }
    }
}
