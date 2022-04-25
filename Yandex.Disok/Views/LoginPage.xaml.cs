using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Ya.D.Helpers;
using Ya.D.Models;
using Ya.D.Services;
using Ya.D.Services.SettingsServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ya.D.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            DataContext = new LoginPageVM();
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void WebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var token = await DataUtils.TryGetToken(args.Uri);
            if (!string.IsNullOrWhiteSpace(token))
            {
                await PassportService.Instance.GetProfileData(token);
                await (DataContext as LoginPageVM).DecideAsync();
            }
        }
    }

    public class LoginPageVM : ViewModelBase
    {
        private bool _useLoginForm;
        private Uri _loginURL;
        private UserInfo _info;

        public bool UseLoginForm { get => _useLoginForm; set => Set(ref _useLoginForm, value); }
        public Uri LoginURL { get => _loginURL; set => Set(ref _loginURL, value); }
        public UserInfo UserInfo { get => _info; set => Set(ref _info, value); }

        public DelegateCommand ReLoginCMD { get; private set; }

        public LoginPageVM()
        {
            ReLoginCMD = new DelegateCommand(() => ReLoginExec());
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            await DecideAsync();
        }

        public async Task DecideAsync()
        {
            Shell.HamburgerMenu.IsFullScreen = true;
            if (!SettingsService.Instance.GotConnection)
            {
                await NavigationService.NavigateAsync(typeof(ErrorPage), new DiskBaseModel { Code = 2, Description = "No Internet connection found. Check your connection or press 'Try again'", Error = "No Internet connection" });
            }
            else if (SettingsService.Instance.CurrentUserData?.LoggedOut ?? false)
            {
                UserInfo = SettingsService.Instance.CurrentUserData;
            }
            else if (SettingsService.Instance.CurrentUserData?.CanUse() ?? false)
            {
                Shell.HamburgerMenu.IsFullScreen = false;
                if (SettingsService.Instance.FirstTimeLaunch)
                    await NavigationService.NavigateAsync(typeof(SettingsPage));
                else
                    await NavigationService.NavigateAsync(typeof(MainPage));
                SettingsService.Instance.FirstTimeLaunch = false;
            }
            else
            {
                LoginURL = new Uri(Commons.YAD_URL);
            }
        }

        private void ReLoginExec()
        {
            UserInfo.LoggedOut = false;
            SettingsService.Instance.CurrentUserData = UserInfo;
            if (SettingsService.Instance.CurrentUserData.CanUse())
            {
                Shell.HamburgerMenu.IsFullScreen = false;
                Shell.NavigationService.Navigate(typeof(MainPage));
            }
            else
            {
                Shell.NavigationService.Navigate(typeof(LoginPage));
            }
        }
    }
}
