using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Ya.D.Models;
using Ya.D.Services;
using Ya.D.Services.SettingsServices;
using Ya.D.Views;

namespace Ya.D.ViewModels
{
    public class ProfileViewModel : ViewModelBase
    {
        private UserInfo _profile;
        private DiskInfo _quota = new DiskInfo();

        public UserInfo Profile { get => _profile; set => Set(ref _profile, value); }
        public DiskInfo Quota { get => _quota; set => Set(ref _quota, value); }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            Profile = await PassportService.Instance.GetProfileData();
            await base.OnNavigatedToAsync(parameter, mode, state);
            var info = await DiskService.Client.GetDiskInfo();
            Quota = info;
        }

        public async Task ClearChacheAsync()
        {
            var dialog = new ContentDialog()
            {
                Title = "Clear application cache?",
                Content = "Are you sure you want to clear application cache?\nAll information about files/folders in Ya.D (but not your files) will be deleted.",
                CloseButtonText = "No",
                PrimaryButtonText = "Clear"
            };
            var dialogResult = await dialog.ShowAsync();
            if (dialogResult != ContentDialogResult.Primary)
            {
                return;
            }

            await DataProvider.Instance.ResetAsync();
        }

        public async Task LogoutAsync()
        {
            var dialog = new ContentDialog()
            {
                Title = "Logout?",
                Content = "Are you sure you want to logout from application?",
                CloseButtonText = "No",
                PrimaryButtonText = "Yes"
            };
            var dialogResult = await dialog.ShowAsync();
            if (dialogResult != ContentDialogResult.Primary)
            {
                return;
            }

            var info = SettingsService.Instance.CurrentUserData;
            info.LoggedOut = true;
            SettingsService.Instance.CurrentUserData = info;

            //await PassportService.Instance.DiscardToken();
            //SettingsService.Instance.CurrentUserData = new UserInfo();
            await NavigationService.NavigateAsync(typeof(LoginPage));

        }
    }
}
