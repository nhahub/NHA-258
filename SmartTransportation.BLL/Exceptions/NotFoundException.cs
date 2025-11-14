namespace SmartTransportation.BLL.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string resourceName, object? key = null) 
            : base(
                key != null 
                    ? $"{resourceName} with key '{key}' was not found." 
                    : $"{resourceName} was not found.",
                statusCode: 404,
                errorCode: "NOT_FOUND")
        {
        }
    }
}

