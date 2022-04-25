using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Ya.D.Models;
using Ya.D.Services;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ya.D.Views.Dialogs
{
    public sealed partial class BrowseDialog : ContentDialog
    {
        public string SelectedFolder { get => ViewModel.PreviousItem?.Path; }

        public BrowseDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            if (ViewModel.SelectedItem != null && ViewModel.SelectedItem.Path == "/")
                return;
            var items = ViewModel.SelectedItem?.Path.Split('/');
            var path = ViewModel.SelectedItem == null ? "/" : items == null ? "/" : string.Join("/", items.Take(items.Length - 1));
            ViewModel.SelectedItem = new DiskItem { Path = path };
        }
    }

    internal class BrowseDialogVM : ViewModelBase
    {
        private bool _loading = true;
        private bool _isEmpty = false;
        private string _path = "Disk /";
        private DiskItem _prevItem;
        private DiskItem _selected = new DiskItem() { Path = "/" };
        private ObservableCollection<DiskItem> _items = new ObservableCollection<DiskItem>();

        public bool IsLoading { get => _loading; set => Set(ref _loading, value); }
        public bool IsEmpty { get => _isEmpty; set => Set(ref _isEmpty, value); }
        public string PathAsString { get => _path; set => Set(ref _path, value); }
        public DiskItem PreviousItem { get => _prevItem; }
        public DiskItem SelectedItem
        {
            get => _selected;
            set
            {
                if (Set(ref _selected, value))
                    Task.Factory.StartNew(async () => await LoadItemsAsync(), CancellationToken.None, TaskCreationOptions.None, DiskService.Client.Sync);
            }
        }

        public ObservableCollection<DiskItem> Items { get => _items; set => Set(ref _items, value); }

        public BrowseDialogVM()
        {
            Task.Run(async () => await LoadItemsAsync());
        }

        private async Task LoadItemsAsync()
        {
            if (_prevItem != null && SelectedItem == null)
                return;
            IsLoading = true;
            _prevItem = SelectedItem;
            Items.Clear();
            var data = await DataProvider.Instance.GetItemsAsync(i => i.ParentFolder.Equals(_selected), null, ignoreActual: true);
            PathAsString = _prevItem.Path == "/" ? "Disk /" : string.Join(" / ", _prevItem.Path.Split('/').Select(i => string.IsNullOrWhiteSpace(i) ? "Disk" : i));
            Items = new ObservableCollection<DiskItem>(data.Where(i => i.IsFolder));
            IsEmpty = Items.Count == 0;
            IsLoading = false;
        }

        public void GridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var panel = (ItemsWrapGrid)(sender as GridView).ItemsPanelRoot;
            var actualState = Shell.Instance.CurrentState;
            if (actualState == null)
                return;
            int gridColumnWidth;
            switch (actualState.Name)
            {
                case "VisualStateNarrow":
                    gridColumnWidth = 3;
                    break;
                case "VisualStateNormal":
                    gridColumnWidth = 6;
                    break;
                default:
                    gridColumnWidth = 9;
                    break;
            }
            panel.ItemWidth = e.NewSize.Width / gridColumnWidth;
        }
    }
}