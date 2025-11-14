namespace SmartTransportation.BLL.Exceptions
{
    public class ValidationException : BaseException
    {
        public Dictionary<string, string[]> Errors { get; set; } = new();

        public ValidationException(Dictionary<string, string[]> errors) 
            : base("One or more validation errors occurred.", statusCode: 400, errorCode: "VALIDATION_ERROR")
        {
            Errors = errors;
        }

        public ValidationException(string field, string error) 
            : base("One or more validation errors occurred.", statusCode: 400, errorCode: "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
        }
    }
}

