using System;
using Ya.D.Models;

namespace Ya.D.Services
{
    public enum ErrorType
    {
        General = 1,
        HTTPRequest,
        HTTPResponse,
        Upload,
        Download,
        Serialization,
    }

    public class ErrorHandler
    {
        private static readonly Lazy<ErrorHandler> _instance = new Lazy<ErrorHandler>(() => new ErrorHandler());
        public static ErrorHandler Instance => _instance.Value;

        private ErrorHandler() { }

        public DiskBaseModel Handle(Exception ex, ErrorType errorType, string additional = "")
        {
            var result = new DiskBaseModel { Code = (int)errorType };

            return result;
        }
    }
}