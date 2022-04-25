using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Ya.D.Models;
using Ya.D.Services;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Ya.D.Views.Dialogs
{
    public sealed partial class AddPlaylistDialog : ContentDialog
    {
        private PlayListType _selectedType;

        public PlayList Result { get; private set; }

        public AddPlaylistDialog()
        {
            InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (_selectedType == null || string.IsNullOrEmpty(PlaylistName.Text))
                return;
            if (_selectedType.ID <= 0)
                _selectedType = await DataProvider.Instance.AddPlayListTypeAsync(_selectedType);
            var preResult = new PlayList() { Name = PlaylistName.Text, TypeID = _selectedType.ID, Type = _selectedType };
            Result = await DataProvider.Instance.AddPlayListAsync(preResult);
            Hide();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                _selectedType = (PlayListType)args.ChosenSuggestion;
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var matchingContacts = DataProvider.Instance.GetPlayListTypesAsync((i) => i.Name.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) > -1);
                sender.ItemsSource = matchingContacts;
                if (matchingContacts.Count == 0)
                    _selectedType = new PlayListType() { Name = sender.Text };
            }
        }
    }
}
