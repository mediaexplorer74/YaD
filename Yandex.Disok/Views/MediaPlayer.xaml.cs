using Windows.UI.Xaml.Navigation;
using Ya.D.Services;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ya.D.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MediaPlayer
    {
        public MediaPlayer()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainMediaElement.SetMediaPlayer(MediaPlayerService.Service.Player);
        }
    }
}
