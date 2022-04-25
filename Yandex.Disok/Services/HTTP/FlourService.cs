using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ya.D.Services.HTTP
{
    public class FlourService
    {
        private readonly string _serviceURL = "https://api.4eto.co.in/api/v1/analytics";
        private readonly HttpClient _client;
        private static readonly Lazy<FlourService> _lazy = new Lazy<FlourService>(() => new FlourService());
        public static FlourService I { get => _lazy.Value; }

        private FlourService()
        {
            _client = new HttpClient();
        }

        public async Task<bool> PushAnalytics(string category, string message)
        {
            category = ReduceString(category);
            message = ReduceString(message);
            var data = new Dictionary<string, string>
                {
                    { "appName", "YaD" },
                    { "appId", SettingsServices.SettingsService.Instance.ApplicationID },
                    { "category", category},
                    { "message", message}
                };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.PostAsync(_serviceURL, content);
                return response.StatusCode == HttpStatusCode.Created;
            }
            catch (Exception)
            {
                return false;
            }            
        }

        private string ReduceString(string text)
            => !string.IsNullOrEmpty(text)
                ? text.Length > 255 ? text.Substring(0, 255) : text
                : string.Empty;
    }
}
