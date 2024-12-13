using System.Security.Cryptography;
using System.Text;

namespace TrafficCourts.Staff.Service.Middleware;

public class ETagMiddleware
{
    private readonly RequestDelegate _next;

    public ETagMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only handle GET requests
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context);
            return;
        }

        // capture the response body
        Stream originalBodyStream = context.Response.Body;

        // create new response body stream
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;
        await _next(context);

        // Only generate ETag for successful responses
        if (context.Response.StatusCode == StatusCodes.Status200OK)
        {
            // Generate ETag based on the response content
            memoryStream.Seek(0, SeekOrigin.Begin);
            var content = await new StreamReader(memoryStream).ReadToEndAsync();
            var eTag = GenerateETag(content);

            // Check the If-None-Match header
            if (context.Request.Headers.TryGetValue("If-None-Match", out Microsoft.Extensions.Primitives.StringValues value) && value == eTag)
            {
                context.Response.StatusCode = StatusCodes.Status304NotModified;
                context.Response.Headers.ETag = eTag;
                context.Response.Body = originalBodyStream;
                context.Response.ContentLength = 0; // Ensure the response body is empty
                return;
            }

            // Include the ETag in the response headers
            context.Response.Headers.ETag = eTag;

            // Write the response body back to the original stream
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }

        context.Response.Body = originalBodyStream;
    }

    private string GenerateETag(string content)
    {
        using (var md5 = MD5.Create())
        {
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
            return $"\"{Convert.ToBase64String(hash)}\"";
        }
    }
}
