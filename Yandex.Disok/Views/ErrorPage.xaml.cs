using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Template10.Mvvm;
using Ya.D.Models;
using Ya.D.Services.SettingsServices;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ya.D.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorPage : Page
    {
        public ErrorPage()
        {
            DataContext = new ErrorPageVM();
            Shell.HamburgerMenu.IsFullScreen = true;
            InitializeComponent();
        }
    }

    public class ErrorPageVM : ViewModelBase
    {
        private DiskBaseModel _model;

        public DiskBaseModel Model { get => _model; set => Set(ref _model, value); }

        public DelegateCommand ActionCMD { get; private set; }

        public ErrorPageVM()
        {
            ActionCMD = new DelegateCommand(() => ActionExec());
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (parameter is DiskBaseModel data)
            {
                Model = data;
            }
            return Task.CompletedTask;
        }

        private void ActionExec()
        {
            switch (Model.Code)
            {
                //case 1:
                //    LoginWithPrevious();
                //    break;
                case 2:
                    CheckConnection();
                    break;
                default:
                    break;
            }
        }

        private void CheckConnection()
        {
            if (SettingsService.Instance.CheckConnections())
                Shell.NavigationService.Navigate(typeof(LoginPage));
        }
    }
}