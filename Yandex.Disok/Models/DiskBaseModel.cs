using Newtonsoft.Json;
using Template10.Mvvm;

namespace Ya.D.Models
{
    public interface IDiskBaseModel
    {
        int Code { get; set; }
        string Error { get; set; }
        string Description { get; set; }
        string PureResponse { get; set; }
        bool IsError();
    }

    public class DiskBaseModel : BindableBase, IDiskBaseModel
    {
        public int Code { get; set; }
        public string Error { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PureResponse { get; set; }

        public bool IsError()
        {
            return !(string.IsNullOrWhiteSpace(Error));
        }
    }
}