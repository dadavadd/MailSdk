namespace MailSdk.Models;

public class Response<T>
{
    public bool Success { get; set; }
    public T Data { get; set; } = default!;
    public string ErrorMessage { get; set; } = string.Empty;

    public Response() => Success = true;

    public Response(string errorMessage, bool success = false)
    {
        Success = success;
        ErrorMessage = errorMessage;
        Data = default!;
    }
}
