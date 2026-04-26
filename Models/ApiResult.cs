namespace SplitMoney.Client.Models
{
    public class ApiResult
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResult Success(string? message = null) => 
            new() { Succeeded = true, Message = message };

        public static ApiResult Failure(string error) => 
            new() { Succeeded = false, Message = error, Errors = new List<string> { error } };

        public static ApiResult Failure(List<string> errors) => 
            new() { Succeeded = false, Message = errors.FirstOrDefault(), Errors = errors };
    }

    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; set; }

        public static ApiResult<T> Success(T data, string? message = null) => 
            new() { Succeeded = true, Data = data, Message = message };

        public static new ApiResult<T> Failure(string error) => 
            new() { Succeeded = false, Message = error, Errors = new List<string> { error } };

        public static new ApiResult<T> Failure(List<string> errors) => 
            new() { Succeeded = false, Message = errors.FirstOrDefault(), Errors = errors };
    }
}
