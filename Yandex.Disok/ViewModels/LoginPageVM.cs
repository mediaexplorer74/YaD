using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Navigation;
using Ya.D.Helpers;
using Ya.D.Services.SettingsServices;

namespace Ya.D.ViewModels
{
    public class LoginPageVM : ViewModelBase
    {
        private bool _showLoginForm;
        private bool _gotConnection;
        private Uri _currentURL;

        public bool ShowLoginForm { get => _showLoginForm; set => Set(ref _showLoginForm, value); }
        public bool GotConnection { get => _gotConnection; set => Set(ref _gotConnection, value); }
        public Uri Source { get => _currentURL; set => Set(ref _currentURL, value); }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            var ShowLoginForm = !SettingsService.Instance.CurrentUserData.LoggedOut;
            var GotConnection = SettingsService.Instance.GotConnection;
            if (ShowLoginForm && GotConnection)
            {
                Source = new Uri(Commons.YAD_URL);
            }

            return base.OnNavigatedToAsync(parameter, mode, state);
        }
    }
}
