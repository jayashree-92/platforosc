
using System.Collections.Generic;

namespace Com.HedgeMark.Commons
{
    public enum StatusType { Success, Failure };

    public class Status
    {
        public static readonly Status OK = new Status();

        public StatusType Type { get; set; }

        public string Message { get; set; }


        public Status()
        {
            Type = StatusType.Success;
            Message = string.Empty;
        }

        public static Status GetFailure(string failureMessage)
        {
            Status status = new Status();
            status.Type = StatusType.Failure;
            status.Message = failureMessage;
            return status;
        }

        public static Status GetSuccess(string successMessage)
        {
            Status status = new Status();
            status.Type = StatusType.Success;
            status.Message = successMessage;
            return status;
        }

        public bool IsSuccess
        {
            get { return Type == StatusType.Success; }
        }

        public bool IsFailure
        {
            get { return Type == StatusType.Failure; }
        }
    }

    public class MultiStatus : Status
    {
        public List<Status> AllStatus = new List<Status>();

        public void Add(Status status)
        {
            AllStatus.Add(status);
            if (status.Type == StatusType.Failure)
                this.Type = StatusType.Failure;

        }

    }


}
