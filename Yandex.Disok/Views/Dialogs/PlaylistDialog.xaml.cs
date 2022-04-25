using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Ya.D.Models;
using Ya.D.Services;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ya.D.Views.Dialogs
{
    public sealed partial class PlaylistDialog : ContentDialog
    {
        private DiskItem _diskItem;
        public List<PlayList> PlayLists { get; set; } = new List<PlayList>();
        public List<PlayList> Result { get; set; } = new List<PlayList>();

        /// <summary>
        /// Show play list dialog to make some action
        /// </summary>
        /// <param name="item">Disk item - audio or video file</param>
        /// <param name="action">What to do - 0 is Add, 1 Remove</param>
        public PlaylistDialog(DiskItem item, int action = 0)
        {
            _diskItem = item;
            InitializeComponent();
            if (action == 0)
            {
                Title = $"Add '{_diskItem.DisplayName}' to playlist";
                PrimaryButtonText = "Add";
                PlayLists = DataProvider.Instance.GetPlaylists(true)
                    .Where(l => l.DiskItems.FirstOrDefault(i => i.Equals(_diskItem)) == null)
                    .ToList();
            }
            else if (action == 1)
            {
                PrimaryButtonText = "Remove";
                PlayLists = DataProvider.Instance.GetPlaylists(true)
                    .Where(l => l.DiskItems.FirstOrDefault(i => i.Equals(_diskItem)) != null)
                    .ToList();
                if (PlayLists.Count == 0)
                    Title = Title = $"'{_diskItem.DisplayName}' does not belong to any of playlists";
                else
                    Title = $"Remove '{_diskItem.DisplayName}' from playlist";
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Result = PlaylistsData.SelectedItems.Select(i => i as PlayList).ToList();
            Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }
    }
}
