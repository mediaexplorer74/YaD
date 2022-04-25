using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Push;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Common;
using Template10.Controls;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Ya.D.Helpers;
using Ya.D.Services;
using Ya.D.Services.HTTP;
using Ya.D.Services.SettingsServices;

namespace Ya.D
{
    /// Documentation on APIs used in this page:
    /// https://github.com/Windows-XAML/Template10/wiki

    [Bindable]
    sealed partial class App : BootStrapper
    {
        private bool firstInit = true;
        public App()
        {
            var settings = SettingsService.Instance;
            InitializeComponent();
            SplashFactory = (e) => new Views.Splash(e);
            UnhandledException += (s, e) => Analytics.TrackEvent(
                "AppCrash",
                new Dictionary<string, string>
                {
                    { "AppCrash", $"For  user: {(string.IsNullOrWhiteSpace(settings.CurrentUserData.DefaultEmail)? "unknonw" : settings.CurrentUserData.DefaultEmail)}" },
                    { "error", e.Message },
                    { "appid", SettingsService.Instance.ApplicationID }
                }
            );
            // app settings
            // some settings must be set in app.constructor            
            RequestedTheme = settings.AppTheme;
            CacheMaxDuration = settings.CacheMaxDuration;
            ShowShellBackButton = settings.UseShellBackButton;
        }

        public override UIElement CreateRootElement(IActivatedEventArgs e)
        {
            var service = NavigationServiceFactory(BackButton.Attach, ExistingContent.Exclude);
            var result = new ModalDialog
            {
                DisableBackButtonWhenModal = true,
                Content = new Views.Shell(service),
                ModalContent = new Views.Busy(),
            };
            return result;
        }

        public override async Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            // TODO: add your long-running task here
            // DB init - to fix
            if (firstInit)
            {
                firstInit = false;
                using (var db = new LocalContext())
                {
                    try
                    {
                        db.Database.Migrate();
                    }
                    catch (Exception)
                    {
                        // first time exception
                    }
                    await db.InitMigrateAsync();
                }
                Init();
            }

            // Get Device Type
            SettingsService.Instance.DeviceType = DeviceTypeHelper.GetDeviceFormFactorType();

            // Check for connection
            SettingsService.Instance.CheckConnections();

            Views.Shell.HamburgerMenu.IsFullScreen = true;

            if (args.Kind == ActivationKind.Protocol)
            {
                var protocolUri = (args as ProtocolActivatedEventArgs).Uri;
                var token = await DataUtils.TryGetToken(protocolUri);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    await PassportService.Instance.GetProfileData(token);
                }
            }
            if (SettingsService.Instance.CurrentUserData?.CanUse() ?? false)
            {
                Views.Shell.HamburgerMenu.IsFullScreen = false;
                await NavigationService.NavigateAsync(typeof(Views.MainPage));
            }
            else
            {
                await NavigationService.NavigateAsync(typeof(Views.LoginPage));
            }
        }

        private void Init()
        {
            // Analitycs - app center
            Task.Factory.StartNew(async () =>
            {
                AppCenter.Start("96734f67-dd26-4f12-a053-3a03f88c43df", typeof(Analytics));
                AppCenter.Start("96734f67-dd26-4f12-a053-3a03f88c43df", typeof(Push));
                Analytics.TrackEvent("AppStart", new Dictionary<string, string> {
                    { "language", Windows.System.UserProfile.GlobalizationPreferences.Languages[0] },
                    { "version", SettingsService.Instance.AppVersion },
                    { "appid", SettingsService.Instance.ApplicationID }
                });

                await FlourService.I.PushAnalytics("language", Windows.System.UserProfile.GlobalizationPreferences.Languages[0]);
                await FlourService.I.PushAnalytics("version", SettingsService.Instance.AppVersion);
                await FlourService.I.PushAnalytics("appid", SettingsService.Instance.ApplicationID);
            });
        }
    }
}
