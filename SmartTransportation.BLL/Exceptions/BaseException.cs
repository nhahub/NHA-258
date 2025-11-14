namespace SmartTransportation.BLL.Exceptions
{
    public abstract class BaseException : Exception
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; } = string.Empty;

        protected BaseException(string message, int statusCode = 500, string errorCode = "GENERAL_ERROR") 
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        protected BaseException(string message, Exception innerException, int statusCode = 500, string errorCode = "GENERAL_ERROR") 
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}

