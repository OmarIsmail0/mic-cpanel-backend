namespace UpdateBackend.Api.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int Count { get; set; }
        public string? Error { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int count = 0)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Count = count
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, string? error = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Error = error
            };
        }
    }
}
