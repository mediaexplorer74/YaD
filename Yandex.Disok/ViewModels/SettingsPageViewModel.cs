using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Ya.D.Helpers;
using Ya.D.Services.SettingsServices;

namespace Ya.D.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public SettingsPartViewModel SettingsPartViewModel { get; } = new SettingsPartViewModel();
        public AboutPartViewModel AboutPartViewModel { get; } = new AboutPartViewModel();
        public FeedbackViewModel FeedbackViewModel { get; } = new FeedbackViewModel();
    }

    public class SettingsPartViewModel : ViewModelBase
    {
        private readonly SettingsService _settings;

        public SettingsPartViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // designtime
            }
            else
            {
                _settings = SettingsService.Instance;
            }
        }

        public bool UseBackButtonSetting { get => SettingsService.Instance.DeviceType != Services.DeviceType.Phone; }

        public bool UseShellBackButton
        {
            get => _settings.UseShellBackButton;
            set { _settings.UseShellBackButton = value; RaisePropertyChanged(); }
        }

        public bool NavigateAfterUpload
        {
            get => _settings.NavigateAfterUpload;
            set => _settings.NavigateAfterUpload = value;
        }

        public bool UseLightThemeButton
        {
            get => _settings.AppTheme.Equals(ApplicationTheme.Light);
            set { _settings.AppTheme = value ? ApplicationTheme.Light : ApplicationTheme.Dark; RaisePropertyChanged(); }
        }

        public bool LoadPreviews
        {
            get => _settings.LoadPreviews;
            set { _settings.LoadPreviews = value; RaisePropertyChanged(); }
        }

        public bool UseHighConnection
        {
            get => _settings.LoadWithGoodInet;
            set { _settings.LoadWithGoodInet = value; RaisePropertyChanged(); }
        }
    }

    public class AboutPartViewModel : ViewModelBase
    {
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Windows.ApplicationModel.Package.Current.DisplayName;

        public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get => SettingsService.Instance.AppVersion;
        }
    }

    public class FeedbackViewModel : ViewModelBase
    {

        private readonly Regex _emailRegex = new Regex(@"[a-z0-9_\.-]+@[a-z0-9_\.-]+\.[a-z0-9_\.-]{2,10}", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        private bool _canSend;
        private string _error;
        private string _name;
        private string _theme;
        private string _email;
        private string _text;

        public bool CanSend { get => _canSend; set => Set(ref _canSend, value); }
        public string Error { get => _error; set => Set(ref _error, value); }
        public string Name { get => _name; set => Set(ref _name, value); }
        public string Theme { get => _theme; set => Set(ref _theme, value); }
        public string Email { get => _email; set => Set(ref _email, value); }
        public string Text { get => _text; set => Set(ref _text, value); }

        public FeedbackViewModel()
        {
            CheckCanSend();
            Name = SettingsService.Instance.CurrentUserData?.DisplayName;
            Email = SettingsService.Instance.CurrentUserData?.Emails.FirstOrDefault();
        }

        public async void SendFeedBack()
        {
            try
            {
                if (!Validate())
                {
                    return;
                }

                var data = new Dictionary<string, string>
                {
                    { "name", _name },
                    { "theme", _theme},
                    { "email", _email},
                    { "text", _text},
                    { "appid", SettingsService.Instance.ApplicationID}
                };

                var client = new HttpClient();
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(Commons.FEEDBACK_URL, content);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    Debug.WriteLine(SettingsService.Instance.ApplicationID);
                    var responseData = await response.Content.ReadAsStringAsync();
                    Analytics.TrackEvent("Feedback", new Dictionary<string, string> {
                        { "reason", $"Could not send feed back - {responseData}" },
                        { "appid", SettingsService.Instance.ApplicationID }
                    });
                    Error = "Could not send feedback";
                    return;
                }
                SettingsService.Instance.LastFeedback = DateTime.Now;
                CheckCanSend();
            }
            catch (Exception)
            {
                Analytics.TrackEvent("Feedback", new Dictionary<string, string>
                {
                    { "name", _name },
                    { "theme", _theme },
                    { "email", _email },
                    { "text", _text },
                    { "appid", SettingsService.Instance.ApplicationID }
                });
            }
        }

        private bool Validate()
        {
            Error = string.Empty;
            if (string.IsNullOrWhiteSpace(_name))
            {
                Error += $"Please type your name. ";
            }
            if (string.IsNullOrWhiteSpace(_theme))
            {
                Error += $"Please type theme of your feedback. ";
            }
            if (!string.IsNullOrWhiteSpace(_email) && !_emailRegex.IsMatch(_email))
            {
                Error += $"'{_email}' is not valid value for E-mail. ";
            }
            if (string.IsNullOrWhiteSpace(_text))
            {
                Error += $"Feedback text cannot be empty. ";
            }
            else if (_text.Length > 250)
            {
                Text = _text.Substring(0, 250);
            }
            return string.IsNullOrWhiteSpace(Error);
        }

        private void CheckCanSend()
        {
#if DEBUG
            CanSend = true;
#else
            CanSend = (DateTime.Now - SettingsService.Instance.LastFeedback).Hours >= 1;
#endif

            Error = CanSend ? string.Empty : $"Please wait for a while before send another feedback. You sent feedback at {SettingsService.Instance.LastFeedback.ToString("yyyy-MM-dd HH:mm")}";
        }
    }
}
