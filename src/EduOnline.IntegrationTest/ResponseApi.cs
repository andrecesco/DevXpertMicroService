namespace EduOnline.IntegrationTest;

public class ResponseApi<TResponse>
{
    public bool Success { get; set; }
    public TResponse? Data { get; set; }
}
