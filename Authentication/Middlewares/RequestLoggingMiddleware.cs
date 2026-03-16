public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/favicon.ico") ||
            !context.Request.ContentType?.Contains("application/json") == true)
        {
            await _next(context);
            return;
        }

        var request = context.Request; //Lấy thông tin request

        //Chuyển request body về dạng string sử dụng StreamReader 
        /*
            - StreamRead được dùng để đọc dữ liệu từ các dạng stream khác nhau như FileStream, MemoryStream, NetworkStream, 
                PipeStream, CryptoStream, BufferedStream, GZipStream, DeflateStream, và các lớp stream khác.
            - Trong trường hợp này là InputStream của request.
         */
        var requestBody = await new StreamReader(request.Body).ReadToEndAsync();

        _logger.LogInformation("🔹 [REQUEST] {Method} {Path} | Body: {Body}",
            request.Method, request.Path, requestBody);

        // Ghi log response
        // Biến originalBodyStream lưu trữ response body (Lúc này vẫn chưa có data)
        var originalBodyStream = context.Response.Body;
        /*
            - Ở đây Response.Body là một stream, nó sẽ lưu trữ dữ liệu trả về từ server.
            - Tuy nhiên Response.Body là một stream write only, nghĩa là chỉ có thể ghi dữ liệu vào stream này, không thể đọc dữ liệu từ stream này.
            - Để đọc dữ liệu từ stream này, ta cần tạo một stream khác để lưu trữ dữ liệu trả về từ server.
         */
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context); // Gọi request tiếp theo trong pipeline

        //Trỏ con tro về đầu stream để đọc dữ liệu (Do response body được ghi vào stream nên vị trí con trỏ bị thay đổi)
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        //Đổ dữ liệu từ responseBody vào originalBodyStream (Trả về stream của response body để trả ra client)
        await responseBody.CopyToAsync(originalBodyStream);

        _logger.LogInformation("🔸 [RESPONSE] {StatusCode} | Body: {ResponseBody}",
            context.Response.StatusCode, responseText);
    }
}
