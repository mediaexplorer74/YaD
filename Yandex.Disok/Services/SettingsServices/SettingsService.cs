using System;
using System.Collections.Generic;
using Template10.Common;
using Template10.Services.SettingsService;
using Template10.Utils;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Ya.D.Models;

namespace Ya.D.Services.SettingsServices
{
    public class SettingsService
    {
        private readonly ISettingsHelper _helper;
        private static readonly Lazy<SettingsService> _instance = new Lazy<SettingsService>(() => new SettingsService());
        public static SettingsService Instance => _instance.Value;

        public bool GotConnection { get; internal set; }
        public bool GotInetConnection { get; internal set; }

        private SettingsService()
        {
            _helper = new SettingsHelper();
        }

        public string AppVersion
        {
            get
            {
                var v = Windows.ApplicationModel.Package.Current.Id.Version;
                return $"{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            }
        }

        #region T10 basic settings

        public bool UseShellBackButton
        {
            get => _helper.Read(nameof(UseShellBackButton), false);
            set
            {
                _helper.Write(nameof(UseShellBackButton), value);
                BootStrapper.Current.NavigationService.GetDispatcherWrapper().Dispatch(() =>
                {
                    BootStrapper.Current.ShowShellBackButton = value;
                    BootStrapper.Current.UpdateShellBackButton();
                });
            }
        }

        public bool ShowHamburgerButton
        {
            get => _helper.Read(nameof(ShowHamburgerButton), true);
            set
            {
                _helper.Write(nameof(ShowHamburgerButton), value);
                Views.Shell.HamburgerMenu.HamburgerButtonVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsFullScreen
        {
            get => _helper.Read(nameof(IsFullScreen), false);
            set
            {
                _helper.Write(nameof(IsFullScreen), value);
                Views.Shell.HamburgerMenu.IsFullScreen = value;
            }
        }

        public ApplicationTheme AppTheme
        {
            get
            {
                var theme = ApplicationTheme.Light;
                var value = _helper.Read(nameof(AppTheme), theme.ToString());
                return Enum.TryParse(value, out theme) ? theme : ApplicationTheme.Dark;
            }
            set
            {
                _helper.Write(nameof(AppTheme), value.ToString());
                ((FrameworkElement)Window.Current.Content).RequestedTheme = value.ToElementTheme();
                Views.Shell.HamburgerMenu.RefreshStyles(value, true);
            }
        }

        public TimeSpan CacheMaxDuration
        {
            get => _helper.Read(nameof(CacheMaxDuration), TimeSpan.FromDays(2));
            set
            {
                _helper.Write(nameof(CacheMaxDuration), value);
                BootStrapper.Current.CacheMaxDuration = value;
            }
        }

        #endregion

        public bool FirstTimeLaunch
        {
            get => _helper.Read(nameof(FirstTimeLaunch), true);
            set => _helper.Write(nameof(FirstTimeLaunch), value);
        }

        public bool NavigateAfterUpload
        {
            get => _helper.Read(nameof(NavigateAfterUpload), false);
            set => _helper.Write(nameof(NavigateAfterUpload), value);
        }

        public bool LoadPreviews
        {
            get => _helper.Read(nameof(LoadPreviews), false);
            set => _helper.Write(nameof(LoadPreviews), value);
        }

        public bool LoadWithGoodInet
        {
            get => _helper.Read(nameof(LoadWithGoodInet), true);
            set => _helper.Write(nameof(LoadWithGoodInet), value);
        }

        public string ApplicationID
        {
            get => _helper.Read(nameof(ApplicationID), Guid.NewGuid().ToString());
            set => _helper.Write(nameof(ApplicationID), value);
        }

        public UserInfo CurrentUserData
        {
            get => _helper.Read(nameof(CurrentUserData), new UserInfo());
            set => _helper.Write(nameof(CurrentUserData), value);
        }

        public List<UserInfo> UsersData
        {
            get => _helper.Read(nameof(UsersData), new List<UserInfo>());
            set => _helper.Write(nameof(UsersData), value);
        }

        public DateTime LastFeedback
        {
            get => _helper.Read(nameof(LastFeedback), DateTime.MinValue);
            set => _helper.Write(nameof(LastFeedback), value);
        }

        public DeviceType DeviceType
        {
            get => _helper.Read(nameof(DeviceType), DeviceType.Desktop);
            set => _helper.Write(nameof(DeviceType), value);
        }

        public bool CheckConnections()
        {
            var allConnections = NetworkInformation.GetConnectionProfiles();
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            Instance.GotConnection = connections != null;
            Instance.GotInetConnection = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return Instance.GotConnection;
        }
    }
}
