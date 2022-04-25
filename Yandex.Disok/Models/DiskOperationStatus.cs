namespace Ya.D.Models
{
    public enum OperationStatus
    {
        Unknown,
        Success,
        Failure,
        InProgress
    }

    public class DiskOperationStatus : DiskBaseModel
    {
        private string _status;

        public string Status { get { return _status; } set { _status = value; } }
        public OperationStatus OperationStatus { get { return GetStatus(); } }

        private OperationStatus GetStatus()
        {
            if (Status == "success")
                return OperationStatus.Success;
            if (Status == "failure")
                return OperationStatus.Failure;
            if (Status == "in-progress")
                return OperationStatus.InProgress;
            return OperationStatus.Unknown;
        }
    }
}
