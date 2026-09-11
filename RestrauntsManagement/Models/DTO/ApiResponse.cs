public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public ApiResponse(bool success, T data, string message = null)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
