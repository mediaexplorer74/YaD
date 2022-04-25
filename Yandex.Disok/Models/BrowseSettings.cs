using Template10.Mvvm;

namespace Ya.D.Models
{
    public class BrowseSettings : BindableBase
    {
        private bool _selectFolder;
        private bool _selectMultiFiles;
        private bool _selectFile = true;
        private string _fromPage = string.Empty;
        private string _searchPath = "/";

        public bool SelectFolder { get => _selectFolder; set => Set(ref _selectFolder, value); }
        public bool SelectMultipleFiles { get => _selectMultiFiles; set => Set(ref _selectMultiFiles, value); }
        public bool SelectFiles { get => _selectFile; set => Set(ref _selectFile, value); }
        public string FromPage { get => _fromPage; set => Set(ref _fromPage, value); }
        public string SearchPath { get => _searchPath; set => Set(ref _searchPath, value?.Replace("disk:", string.Empty)); }
    }
}
